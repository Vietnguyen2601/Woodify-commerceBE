using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Entity Payment - gateway payment records (PayOS, MoMo, VNPay).
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
    /// JSON array of order GUIDs for multi-order checkout (same as create-payment OrderIds).
    /// </summary>
    public string? RelatedOrderIdsJson { get; set; }

    /// <summary>
    /// Customer account id.
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Tên provider thanh toán: PAYOS, MOMO, VNPAY
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Provider payment id (e.g. PayOS orderCode).
    /// </summary>
    public string? ProviderPaymentId { get; set; }

    /// <summary>
    /// Amount in VND.
    /// </summary>
    public long AmountVnd { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "VND";

    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

    /// <summary>
    /// Created at (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated at (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Raw provider JSON for audit.
    /// </summary>
    public string? ProviderResponse { get; set; }
}
