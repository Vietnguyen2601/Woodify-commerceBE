using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Services;

/// <summary>
/// Handler xử lý PayOS webhook callback
/// </summary>
public class PayOsWebhookHandler : IPayOsWebhookHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly PayOsOptions _payOsOptions;
    private readonly ILogger<PayOsWebhookHandler> _logger;

    public PayOsWebhookHandler(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IOptions<PayOsOptions> payOsOptions,
        ILogger<PayOsWebhookHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _payOsOptions = payOsOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Xử lý webhook từ PayOS
    /// </summary>
    public async Task<PayOsWebhookResponse> HandleWebhookAsync(PayOsWebhookData webhook)
    {
        try
        {
            _logger.LogInformation("PayOS webhook received. OrderCode: {OrderCode}, Status: {Status}",
                webhook.Data?.OrderCode, webhook.Data?.Status);

            // Validate webhook
            if (webhook?.Data == null)
            {
                _logger.LogWarning("Invalid webhook data");
                return new PayOsWebhookResponse { Code = "01", Desc = "Invalid data" };
            }

            // Verify signature (skip nếu là manual confirm hoặc auto-polling)
            if (webhook.Signature?.Equals("manual-confirm", StringComparison.Ordinal) != true &&
                webhook.Signature?.Equals("auto-polling", StringComparison.Ordinal) != true)
            {
                if (!VerifySignature(webhook))
                {
                    _logger.LogWarning("Webhook signature verification failed for orderCode: {OrderCode}",
                        webhook.Data.OrderCode);
                    return new PayOsWebhookResponse { Code = "01", Desc = "Signature verification failed" };
                }

                _logger.LogInformation("Webhook signature verified. OrderCode: {OrderCode}", webhook.Data.OrderCode);
            }
            else
            {
                _logger.LogInformation("Webhook from internal source (manual confirm or auto-polling). OrderCode: {OrderCode}",
                    webhook.Data.OrderCode);
            }

            // Find payment
            var payment = await _paymentRepository.GetByProviderPaymentIdAsync(webhook.Data.OrderCode.ToString());
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for orderCode: {OrderCode}", webhook.Data.OrderCode);
                return new PayOsWebhookResponse { Code = "01", Desc = "Payment not found" };
            }

            _logger.LogInformation("Payment found: {PaymentId}, Status: {Status}", payment.PaymentId, payment.Status);

            // Handle based on webhook status
            if (webhook.Data.Status?.ToUpper() == "PAID")
            {
                await HandleSuccessfulPaymentAsync(payment, webhook.Data);
            }
            else if (webhook.Data.Status?.ToUpper() == "CANCELLED")
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("Payment cancelled: {PaymentId}", payment.PaymentId);
            }

            return new PayOsWebhookResponse { Code = "00", Desc = "success" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PayOS webhook");
            return new PayOsWebhookResponse { Code = "01", Desc = $"Error: {ex.Message}" };
        }
    }

    #region Private Methods

    /// <summary>
    /// Xử lý payment thành công
    /// </summary>
    private async Task HandleSuccessfulPaymentAsync(Payment payment, PayOsWebhookPaymentData data)
    {
        _logger.LogInformation("Processing successful payment: {PaymentId}, Amount: {Amount}",
            payment.PaymentId, data.Amount);

        // Tìm wallet by account_id
        var wallet = await _walletRepository.GetByAccountIdAsync(payment.AccountId!.Value);
        if (wallet == null)
        {
            _logger.LogWarning("Wallet not found for accountId: {AccountId}", payment.AccountId);
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);
            return;
        }

        // Lưu balance trước
        var balanceBefore = wallet.BalanceCents;

        // Cộng tiền vào ví
        wallet.BalanceCents += payment.AmountCents;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);

        // Tạo WalletTransaction
        var transaction = new WalletTransaction
        {
            WalletTxId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            TxType = WalletTransactionType.Credit,
            AmountCents = payment.AmountCents,
            BalanceBeforeCents = balanceBefore,
            BalanceAfterCents = wallet.BalanceCents,
            RelatedPaymentId = payment.PaymentId,
            Status = WalletTransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Note = $"Top-up from {payment.Provider}, OrderCode: {data.OrderCode}"
        };

        await _walletRepository.AddTransactionAsync(transaction);

        // Update payment status
        payment.Status = PaymentStatus.Succeeded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation(
            "Payment processed successfully. WalletId: {WalletId}, TransactionId: {TransactionId}, NewBalance: {Balance}",
            wallet.WalletId, transaction.WalletTxId, wallet.BalanceCents);
    }

    /// <summary>
    /// Verify PayOS webhook signature
    /// </summary>
    private bool VerifySignature(PayOsWebhookData webhook)
    {
        try
        {
            if (string.IsNullOrEmpty(webhook.Signature) || webhook.Data == null)
            {
                _logger.LogWarning("Missing signature or data in webhook");
                return false;
            }

            // Build data to verify (same order as PayOS docs)
            var dataToVerify = webhook.Data.OrderCode
                + webhook.Data.Amount
                + webhook.Data.Description
                + webhook.Data.OrderCode
                + webhook.Data.Reference;

            _logger.LogDebug("Webhook signature verification - Data: {Data}, Signature: {Signature}",
                dataToVerify, webhook.Signature);

            // Verify HMAC
            var checksumKeyBytes = Convert.FromHexString(_payOsOptions.ChecksumKey);
            using var hmac = new HMACSHA256(checksumKeyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToVerify));
            var calculatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var isValid = calculatedSignature.Equals(webhook.Signature, StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation("Webhook signature verification: {Result}. Calculated: {Calculated}, Provided: {Provided}",
                isValid, calculatedSignature, webhook.Signature);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    #endregion
}
