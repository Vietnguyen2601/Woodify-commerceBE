using System.Text.Json.Serialization;

namespace PaymentService.Application.DTOs;

/// <summary>
/// PayOS webhook callback data
/// </summary>
public class PayOsWebhookData
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }
    [JsonPropertyName("data")]
    public PayOsWebhookPaymentData? Data { get; set; }
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}

/// <summary>
/// Payment data từ PayOS webhook
/// </summary>
public class PayOsWebhookPaymentData
{
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }
    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }
    [JsonPropertyName("transactionDateTime")]
    public string? TransactionDateTime { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("code")]
    public string? Code { get; set; }
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }
}

/// <summary>
/// Response confirm webhook
/// </summary>
public class PayOsWebhookResponse
{
    public string Code { get; set; } = "00";
    public string Desc { get; set; } = "success";
}
