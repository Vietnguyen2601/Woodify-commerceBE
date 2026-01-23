namespace PaymentService.Application.Interfaces;

/// <summary>
/// Interface cho PayOS Service - dễ mock cho testing và mở rộng
/// Định nghĩa ở Application layer, implementation ở Infrastructure layer
/// </summary>
public interface IPayOsService
{
    /// <summary>
    /// Tạo link thanh toán PayOS
    /// </summary>
    /// <param name="request">Thông tin tạo payment</param>
    /// <returns>Response chứa paymentUrl</returns>
    Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(PayOsCreatePaymentInput request);

    /// <summary>
    /// Verify checksum từ PayOS webhook
    /// </summary>
    /// <param name="data">Data từ webhook</param>
    /// <param name="signature">Signature từ header</param>
    /// <returns>True nếu valid</returns>
    bool VerifyWebhookSignature(string data, string signature);

    /// <summary>
    /// Query thông tin payment từ PayOS
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng</param>
    /// <returns>Thông tin payment</returns>
    Task<PayOsPaymentInfoResult?> GetPaymentInfoAsync(long orderCode);

    /// <summary>
    /// Generate checksum cho request
    /// </summary>
    string GenerateSignature(string data);
}

#region PayOS DTOs for IPayOsService

/// <summary>
/// Input cho create payment link
/// </summary>
public class PayOsCreatePaymentInput
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
}

/// <summary>
/// Result từ create payment link
/// </summary>
public class PayOsCreatePaymentResult
{
    public bool IsSuccess { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCodeUrl { get; set; }
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string? Status { get; set; }
    public string? RawResponse { get; set; }
}

/// <summary>
/// Result từ get payment info
/// </summary>
public class PayOsPaymentInfoResult
{
    public bool IsSuccess { get; set; }
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public int AmountPaid { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}

#endregion
