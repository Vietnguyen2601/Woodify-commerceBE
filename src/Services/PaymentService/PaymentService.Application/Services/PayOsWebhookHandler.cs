using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Events;

namespace PaymentService.Application.Services;

/// <summary>
/// Handler xử lý PayOS webhook callback
/// </summary>
public class PayOsWebhookHandler : IPayOsWebhookHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentEventsPublisher _paymentEventsPublisher;
    private readonly PayOsOptions _payOsOptions;
    private readonly ILogger<PayOsWebhookHandler> _logger;

    public PayOsWebhookHandler(
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IPaymentEventsPublisher paymentEventsPublisher,
        IOptions<PayOsOptions> payOsOptions,
        ILogger<PayOsWebhookHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _paymentEventsPublisher = paymentEventsPublisher;
        _payOsOptions = payOsOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Xử lý webhook PayOS
    /// </summary>
    public async Task<PayOsWebhookResponse> HandleWebhookAsync(PayOsWebhookData webhook)
    {
        try
        {
            _logger.LogInformation("PayOS webhook received. OrderCode: {OrderCode}, Status: {Status}",
                webhook.Data?.OrderCode, webhook.Data?.Status);

            if (webhook?.Data == null)
            {
                _logger.LogWarning("Invalid webhook data");
                return new PayOsWebhookResponse { Code = "01", Desc = "Invalid data" };
            }

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

            var payment = await _paymentRepository.GetByProviderPaymentIdAsync(webhook.Data.OrderCode.ToString());
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for orderCode: {OrderCode}", webhook.Data.OrderCode);
                return new PayOsWebhookResponse { Code = "01", Desc = "Payment not found" };
            }

            _logger.LogInformation("Payment found: {PaymentId}, Status: {Status}", payment.PaymentId, payment.Status);

            if (webhook.Data.Status?.ToUpper() == "PAID")
            {
                if (payment.Status == PaymentStatus.Succeeded)
                {
                    _logger.LogInformation("Payment already Succeeded, skip: {PaymentId}", payment.PaymentId);
                    return new PayOsWebhookResponse { Code = "00", Desc = "success" };
                }

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

    private async Task HandleSuccessfulPaymentAsync(Payment payment, PayOsWebhookPaymentData data)
    {
        _logger.LogInformation("Processing successful payment: {PaymentId}, Amount: {Amount}",
            payment.PaymentId, data.Amount);

        var linkedOrderIds = ResolveLinkedOrderIds(payment);
        if (linkedOrderIds.Count > 0)
        {
            if (!payment.AccountId.HasValue)
            {
                _logger.LogWarning("Payment {PaymentId} linked to orders but AccountId is missing", payment.PaymentId);
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
                return;
            }

            long providerCode = 0;
            long.TryParse(payment.ProviderPaymentId, out providerCode);

            _paymentEventsPublisher.PublishPaymentOrdersPaid(new PaymentOrdersPaidEvent
            {
                PaymentId = payment.PaymentId,
                AccountId = payment.AccountId,
                OrderIds = linkedOrderIds,
                Provider = payment.Provider ?? "PAYOS",
                ProviderOrderCode = providerCode,
                AmountVnd = payment.AmountVnd,
                PaidAt = DateTime.UtcNow
            });

            payment.Status = PaymentStatus.Succeeded;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);
            return;
        }

        // Wallet top-up (no shop orders on this payment)
        var wallet = await _walletRepository.GetByAccountIdAsync(payment.AccountId!.Value);
        if (wallet == null)
        {
            _logger.LogWarning("Wallet not found for accountId: {AccountId}", payment.AccountId);
            payment.Status = PaymentStatus.Failed;
            payment.UpdatedAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);
            return;
        }

        var balanceBefore = wallet.BalanceVnd;

        wallet.BalanceVnd += payment.AmountVnd;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _walletRepository.UpdateAsync(wallet);

        var transaction = new WalletTransaction
        {
            WalletTxId = Guid.NewGuid(),
            WalletId = wallet.WalletId,
            TxType = WalletTransactionType.Credit,
            AmountVnd = payment.AmountVnd,
            BalanceBeforeVnd = balanceBefore,
            BalanceAfterVnd = wallet.BalanceVnd,
            RelatedPaymentId = payment.PaymentId,
            Status = WalletTransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Note = $"Top-up from {payment.Provider}, OrderCode: {data.OrderCode}"
        };

        await _walletRepository.AddTransactionAsync(transaction);

        payment.Status = PaymentStatus.Succeeded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepository.UpdateAsync(payment);

        _logger.LogInformation(
            "Payment processed successfully. WalletId: {WalletId}, TransactionId: {TransactionId}, NewBalance: {Balance}",
            wallet.WalletId, transaction.WalletTxId, wallet.BalanceVnd);
    }

    private static List<Guid> ResolveLinkedOrderIds(Payment payment)
    {
        if (!string.IsNullOrWhiteSpace(payment.RelatedOrderIdsJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<Guid>>(payment.RelatedOrderIdsJson);
                if (parsed is { Count: > 0 })
                    return parsed.Distinct().ToList();
            }
            catch
            {
                /* ignore invalid json */
            }
        }

        if (payment.OrderId.HasValue)
            return new List<Guid> { payment.OrderId.Value };

        return new List<Guid>();
    }

    private bool VerifySignature(PayOsWebhookData webhook)
    {
        try
        {
            if (string.IsNullOrEmpty(webhook.Signature) || webhook.Data == null)
            {
                _logger.LogWarning("Missing signature or data in webhook");
                return false;
            }

            var dataToVerify = webhook.Data.OrderCode
                + webhook.Data.Amount
                + webhook.Data.Description
                + webhook.Data.OrderCode
                + webhook.Data.Reference;

            _logger.LogDebug("Webhook signature verification - Data: {Data}, Signature: {Signature}",
                dataToVerify, webhook.Signature);

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
