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
    public async Task<PayOsWebhookResponse> HandleWebhookAsync(PayOsWebhookData webhook, JsonElement? rawData = null)
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
                if (!VerifySignature(webhook, rawData))
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

            var normalizedStatus = ResolveWebhookStatus(webhook);

            if (normalizedStatus == "PAID")
            {
                if (payment.Status == PaymentStatus.Succeeded)
                {
                    _logger.LogInformation("Payment already Succeeded, skip: {PaymentId}", payment.PaymentId);
                    return new PayOsWebhookResponse { Code = "00", Desc = "success" };
                }

                await HandleSuccessfulPaymentAsync(payment, webhook.Data);
            }
            else if (normalizedStatus == "CANCELLED")
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("Payment cancelled: {PaymentId}", payment.PaymentId);
            }
            else
            {
                _logger.LogWarning("Unsupported PayOS status: {Status} for orderCode: {OrderCode}",
                    webhook.Data.Status, webhook.Data.OrderCode);
                return new PayOsWebhookResponse { Code = "01", Desc = $"Unsupported status: {webhook.Data.Status}" };
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

    private bool VerifySignature(PayOsWebhookData webhook, JsonElement? rawData)
    {
        try
        {
            if (string.IsNullOrEmpty(webhook.Signature) || webhook.Data == null)
            {
                _logger.LogWarning("Missing signature or data in webhook");
                return false;
            }

            var providedSignature = webhook.Signature.Trim();
            var canonical = rawData.HasValue && rawData.Value.ValueKind == JsonValueKind.Object
                ? BuildCanonicalDataString(rawData.Value)
                : BuildCanonicalDataString(webhook.Data);

            var calculated = GenerateHmacSignature(canonical);
            var isValid = calculated.Equals(providedSignature, StringComparison.OrdinalIgnoreCase);
            _logger.LogInformation(
                "Webhook signature verification: {Result}. CanonicalData: {CanonicalData}. Calculated: {Calculated}. Provided: {Provided}",
                isValid, canonical, calculated, providedSignature);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    private static string ResolveWebhookStatus(PayOsWebhookData webhook)
    {
        var status = webhook.Data?.Status?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(status))
            return status;

        var dataCode = webhook.Data?.Code?.Trim();
        if (string.Equals(dataCode, "00", StringComparison.OrdinalIgnoreCase))
            return "PAID";

        var rootCode = webhook.Code?.Trim();
        if (string.Equals(rootCode, "00", StringComparison.OrdinalIgnoreCase))
            return "PAID";

        return string.Empty;
    }

    private static string BuildCanonicalDataString(JsonElement dataObject)
    {
        var pairs = dataObject.EnumerateObject()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"{p.Name}={ConvertJsonValueToCanonicalString(p.Value)}");
        return string.Join("&", pairs);
    }

    private static string BuildCanonicalDataString(PayOsWebhookPaymentData data)
    {
        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["accountName"] = data.AccountName ?? string.Empty,
            ["accountNumber"] = data.AccountNumber ?? string.Empty,
            ["amount"] = data.Amount.ToString(),
            ["code"] = data.Code ?? string.Empty,
            ["currency"] = data.Currency ?? string.Empty,
            ["desc"] = data.Desc ?? string.Empty,
            ["description"] = data.Description ?? string.Empty,
            ["orderCode"] = data.OrderCode.ToString(),
            ["reference"] = data.Reference ?? string.Empty,
            ["status"] = data.Status ?? string.Empty,
            ["transactionDateTime"] = data.TransactionDateTime ?? string.Empty
        };

        return string.Join("&", fields.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    private static string ConvertJsonValueToCanonicalString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.GetRawText()
        };
    }

    private string GenerateHmacSignature(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_payOsOptions.ChecksumKey);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    #endregion
}
