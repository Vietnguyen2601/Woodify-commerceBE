using System.Text.Json.Serialization;

namespace PaymentService.Infrastructure.PayOs.Models;

/// <summary>
/// Payload webhook từ PayOS
/// </summary>
public class PayOsWebhookPayload
{
    /// <summary>
    /// Mã lỗi
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả
    /// </summary>
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    /// <summary>
    /// Dữ liệu webhook
    /// </summary>
    [JsonPropertyName("data")]
    public PayOsWebhookData? Data { get; set; }

    /// <summary>
    /// Signature để verify
    /// </summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    /// <summary>
    /// Check xem webhook có thành công không
    /// </summary>
    public bool IsSuccess => Code == "00";
}

public class PayOsWebhookData
{
    /// <summary>
    /// Mã đơn hàng
    /// </summary>
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    /// <summary>
    /// Số tiền thanh toán
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// Mô tả
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Account number
    /// </summary>
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Reference number
    /// </summary>
    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    /// <summary>
    /// Thời gian giao dịch
    /// </summary>
    [JsonPropertyName("transactionDateTime")]
    public string? TransactionDateTime { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Payment link ID
    /// </summary>
    [JsonPropertyName("paymentLinkId")]
    public string? PaymentLinkId { get; set; }

    /// <summary>
    /// Mã thanh toán
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Mô tả mã
    /// </summary>
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    /// <summary>
    /// Tên ngân hàng đối tác
    /// </summary>
    [JsonPropertyName("counterAccountBankName")]
    public string? CounterAccountBankName { get; set; }

    /// <summary>
    /// Số tài khoản đối tác
    /// </summary>
    [JsonPropertyName("counterAccountNumber")]
    public string? CounterAccountNumber { get; set; }

    /// <summary>
    /// Tên tài khoản đối tác
    /// </summary>
    [JsonPropertyName("counterAccountName")]
    public string? CounterAccountName { get; set; }

    /// <summary>
    /// Virtual account number
    /// </summary>
    [JsonPropertyName("virtualAccountNumber")]
    public string? VirtualAccountNumber { get; set; }

    /// <summary>
    /// Virtual account name
    /// </summary>
    [JsonPropertyName("virtualAccountName")]
    public string? VirtualAccountName { get; set; }
}
