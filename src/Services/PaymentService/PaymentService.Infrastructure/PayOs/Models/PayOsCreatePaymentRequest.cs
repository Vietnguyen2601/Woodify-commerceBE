using System.Text.Json.Serialization;

namespace PaymentService.Infrastructure.PayOs.Models;

/// <summary>
/// Request tạo payment link PayOS
/// </summary>
public class PayOsCreatePaymentRequest
{
    /// <summary>
    /// Mã đơn hàng unique (số nguyên dương, tối đa 9007199254740991)
    /// </summary>
    [JsonPropertyName("orderCode")]
    public long OrderCode { get; set; }

    /// <summary>
    /// Số tiền thanh toán (VND)
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// Mô tả đơn hàng (tối đa 25 ký tự)
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL redirect khi thanh toán thành công
    /// </summary>
    [JsonPropertyName("returnUrl")]
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL redirect khi hủy thanh toán
    /// </summary>
    [JsonPropertyName("cancelUrl")]
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Tên người mua (optional)
    /// </summary>
    [JsonPropertyName("buyerName")]
    public string? BuyerName { get; set; }

    /// <summary>
    /// Email người mua (optional)
    /// </summary>
    [JsonPropertyName("buyerEmail")]
    public string? BuyerEmail { get; set; }

    /// <summary>
    /// SĐT người mua (optional)
    /// </summary>
    [JsonPropertyName("buyerPhone")]
    public string? BuyerPhone { get; set; }

    /// <summary>
    /// Địa chỉ người mua (optional)
    /// </summary>
    [JsonPropertyName("buyerAddress")]
    public string? BuyerAddress { get; set; }

    /// <summary>
    /// Thời gian hết hạn link thanh toán (Unix timestamp, optional)
    /// </summary>
    [JsonPropertyName("expiredAt")]
    public long? ExpiredAt { get; set; }

    /// <summary>
    /// Signature của request
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách items (optional)
    /// </summary>
    [JsonPropertyName("items")]
    public List<PayOsItem>? Items { get; set; }
}

/// <summary>
/// Item trong đơn hàng
/// </summary>
public class PayOsItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }
}
