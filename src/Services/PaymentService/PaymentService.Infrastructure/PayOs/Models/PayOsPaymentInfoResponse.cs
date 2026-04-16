using System.Text.Json.Serialization;

namespace PaymentService.Infrastructure.PayOs.Models;

/// <summary>
/// Response khi query thông tin payment từ PayOS
/// </summary>
public class PayOsPaymentInfoResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [JsonPropertyName("data")]
    public PayOsPaymentInfoData? Data { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    public bool IsSuccess => Code == "00";
}

public class PayOsPaymentInfoData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("amountPaid")]
    public int AmountPaid { get; set; }

    [JsonPropertyName("amountRemaining")]
    public int AmountRemaining { get; set; }

    /// <summary>
    /// Trạng thái: PENDING, PAID, CANCELLED, EXPIRED
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("transactions")]
    public List<PayOsTransaction>? Transactions { get; set; }

    [JsonPropertyName("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [JsonPropertyName("cancellationReason")]
    public string? CancellationReason { get; set; }
}

public class PayOsTransaction
{
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("transactionDateTime")]
    public DateTime? TransactionDateTime { get; set; }

    [JsonPropertyName("virtualAccountName")]
    public string? VirtualAccountName { get; set; }

    [JsonPropertyName("virtualAccountNumber")]
    public string? VirtualAccountNumber { get; set; }

    [JsonPropertyName("counterAccountBankId")]
    public string? CounterAccountBankId { get; set; }

    [JsonPropertyName("counterAccountBankName")]
    public string? CounterAccountBankName { get; set; }

    [JsonPropertyName("counterAccountName")]
    public string? CounterAccountName { get; set; }

    [JsonPropertyName("counterAccountNumber")]
    public string? CounterAccountNumber { get; set; }
}
