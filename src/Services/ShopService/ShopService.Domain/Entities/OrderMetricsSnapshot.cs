namespace ShopService.Domain.Entities;

/// <summary>
/// Snapshot của order metrics từ OrderService events
/// Được cập nhật real-time bởi event consumers
/// Dùng cho dashboard analytics và reporting
/// </summary>
public class OrderMetricsSnapshot
{
    /// <summary>ID duy nhất của record</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID của Shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>ID của Order từ OrderService</summary>
    public Guid OrderId { get; set; }

    /// <summary>Trạng thái order hiện tại</summary>
    public string Status { get; set; } = string.Empty; // PENDING, CONFIRMED, PROCESSING, READY_TO_SHIP, COMPLETED, etc.

    /// <summary>Tổng tiền order (VND, đơn vị: cents)</summary>
    public long TotalAmountCents { get; set; }

    /// <summary>Tiền hoa hồng (VND, đơn vị: cents)</summary>
    public long CommissionCents { get; set; }

    /// <summary>Tiền ròng shop nhận được</summary>
    public long NetAmountCents { get; set; }

    /// <summary>Ngày tạo order (UTC)</summary>
    public DateTime OrderCreatedAt { get; set; }

    /// <summary>Ngày hoàn tất order (UTC)</summary>
    public DateTime? OrderCompletedAt { get; set; }

    /// <summary>Ngày hoàn tiền (nếu có)</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>Số tiền hoàn tiền (nếu có)</summary>
    public long? RefundAmountCents { get; set; }

    /// <summary>Lý do hoàn tiền</summary>
    public string? RefundReason { get; set; }

    /// <summary>Đó là hàng trả lại</summary>
    public bool IsReturn { get; set; }

    /// <summary>Sắp vượt quá SLA (45 phút)</summary>
    public bool IsSLAViolated { get; set; }

    /// <summary>Số lượng sản phẩm trong đơn</summary>
    public int ItemCount { get; set; }

    /// <summary>ID phiên bản sản phẩm chính (từ ProductVersion)</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }

    /// <summary>Timestamp khi record được tạo</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp khi record được cập nhật lần cuối</summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gia trị năm của order (cho group by)</summary>
    public int OrderYear { get; set; }

    /// <summary>Giá trị tháng của order</summary>
    public int OrderMonth { get; set; }

    /// <summary>Giá trị ngày của order</summary>
    public int OrderDay { get; set; }
}
