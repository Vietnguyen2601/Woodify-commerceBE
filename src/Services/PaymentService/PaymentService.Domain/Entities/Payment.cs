using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Entity Payment - lưu thông tin thanh toán từ các provider (PayOS, MoMo, VNPay)
/// </summary>
public class Payment
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// ID đơn hàng (FK to Orders service)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// ID tài khoản người dùng (FK to Accounts service)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Tên provider thanh toán: PAYOS, MOMO, VNPAY
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// ID thanh toán từ provider (orderCode của PayOS)
    /// </summary>
    public string? ProviderPaymentId { get; set; }

    /// <summary>
    /// Số tiền thanh toán (đơn vị: cents/đồng)
    /// </summary>
    public long AmountCents { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    public string Currency { get; set; } = "VND";

    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Response JSON từ provider (dùng để debug/audit)
    /// </summary>
    public string? ProviderResponse { get; set; }
}
