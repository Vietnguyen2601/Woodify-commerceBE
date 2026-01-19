namespace PaymentService.Domain.Enums;

/// <summary>
/// Trạng thái thanh toán - dễ mở rộng cho các provider khác (MoMo, VNPay)
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Đã tạo link thanh toán
    /// </summary>
    Created = 0,

    /// <summary>
    /// Đang xử lý thanh toán
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Thanh toán thành công
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// Thanh toán thất bại
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Đã hoàn tiền
    /// </summary>
    Refunded = 4
}
