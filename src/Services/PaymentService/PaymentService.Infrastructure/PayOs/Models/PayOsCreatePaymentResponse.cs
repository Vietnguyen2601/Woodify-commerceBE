using System.Text.Json.Serialization;

namespace PaymentService.Infrastructure.PayOs.Models;

/// <summary>
/// Response từ PayOS khi tạo payment link
/// </summary>
public class PayOsCreatePaymentResponse
{
    /// <summary>
    /// Mã lỗi (00 = thành công)
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả lỗi
    /// </summary>
    [JsonPropertyName("desc")]
    public string? Desc { get; set; }

    /// <summary>
    /// Data chứa thông tin payment
    /// </summary>
    [JsonPropertyName("data")]
    public PayOsCreatePaymentData? Data { get; set; }

    /// <summary>
    /// Signature của response
    /// </summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    /// <summary>
    /// Kiểm tra response có thành công không
    /// </summary>
    public bool IsSuccess => Code == "00";
}

public class PayOsCreatePaymentData
{
    /// <summary>
    /// Bin của thẻ
    /// </summary>
    [JsonPropertyName("bin")]
    public string? Bin { get; set; }

    /// <summary>
    /// Account number
    /// </summary>
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    [JsonPropertyName("accountName")]
    public string? AccountName { get; set; }

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
    /// Mã đơn hàng
    /// </summary>
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

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
    /// Trạng thái: PENDING, PAID, CANCELLED
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// URL thanh toán - redirect user đến đây
    /// </summary>
    [JsonPropertyName("checkoutUrl")]
    public string? CheckoutUrl { get; set; }

    /// <summary>
    /// URL QR code
    /// </summary>
    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }
}
