using ShipmentService.Domain.Enums;

namespace ShipmentService.Domain.Entities;

/// <summary>
/// Entity Shipment - Bảng Shipments
/// Quản lý thông tin vận chuyển của đơn hàng
/// </summary>
public class Shipment
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public Guid ShipmentId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID đơn hàng (FK to Orders service)
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Mã vận đơn nội bộ (unique, bắt buộc)
    /// </summary>
    public string ShipmentCode { get; set; } = string.Empty;

    /// <summary>
    /// ID dịch vụ vận chuyển của provider (FK to ProviderService)
    /// </summary>
    public Guid? ProviderServiceId { get; set; }

    /// <summary>
    /// Navigation property đến ProviderService
    /// </summary>
    public ProviderService? ProviderService { get; set; }

    /// <summary>
    /// Mã tracking từ provider (unique)
    /// </summary>
    public string? TrackingNumber { get; set; }

    // ─── Địa chỉ ────────────────────────────────────────────────────────────

    /// <summary>
    /// ID địa chỉ lấy hàng (lưu dưới dạng string để linh hoạt giữa services)
    /// </summary>
    public string? PickupAddressId { get; set; }

    /// <summary>
    /// ID địa chỉ giao hàng (lưu dưới dạng string để linh hoạt giữa services)
    /// </summary>
    public string? DeliveryAddressId { get; set; }

    // ─── Thông tin kiện hàng ─────────────────────────────────────────────────

    /// <summary>
    /// Tổng khối lượng (đơn vị: gram)
    /// </summary>
    public double TotalWeightGrams { get; set; }

    /// <summary>
    /// Tổng thể tích (đơn vị: cm³)
    /// </summary>
    public double? TotalVolumeCm3 { get; set; }

    /// <summary>
    /// Số lượng kiện hàng
    /// </summary>
    public int PackageCount { get; set; } = 1;

    /// <summary>
    /// Loại kiện hàng: NORMAL, BULKY, SUPER_BULKY
    /// </summary>
    public BulkyType BulkyType { get; set; } = BulkyType.Normal;

    /// <summary>
    /// Hàng dễ vỡ
    /// </summary>
    public bool IsFragile { get; set; } = false;

    /// <summary>
    /// Yêu cầu bảo hiểm
    /// </summary>
    public bool RequiresInsurance { get; set; } = false;

    /// <summary>
    /// Phí bảo hiểm (đơn vị: cents/đồng)
    /// </summary>
    public double InsuranceFeeCents { get; set; } = 0;

    // ─── Phí vận chuyển ──────────────────────────────────────────────────────

    /// <summary>
    /// Phí vận chuyển thực tế (đơn vị: cents/đồng)
    /// </summary>
    public long FinalShippingFeeCents { get; set; }

    /// <summary>
    /// Miễn phí vận chuyển
    /// </summary>
    public bool IsFreeShipping { get; set; } = false;

    // ─── Thời gian ───────────────────────────────────────────────────────────

    /// <summary>
    /// Thời gian lên lịch lấy hàng
    /// </summary>
    public DateTime? PickupScheduledAt { get; set; }

    /// <summary>
    /// Thời gian đã lấy hàng thực tế
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// Thời gian dự kiến giao hàng
    /// </summary>
    public DateTime? DeliveryEstimatedAt { get; set; }

    /// <summary>
    /// Thời gian giao hàng thành công thực tế
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Thời gian hoàn hàng
    /// </summary>
    public DateTime? ReturnedAt { get; set; }

    // ─── Trạng thái ──────────────────────────────────────────────────────────

    /// <summary>
    /// Trạng thái vận chuyển
    /// </summary>
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    // ─── Thông tin phụ ───────────────────────────────────────────────────────

    /// <summary>
    /// Lý do giao hàng thất bại
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Lý do hủy
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// Ghi chú của khách hàng
    /// </summary>
    public string? CustomerNote { get; set; }

    /// <summary>
    /// Ghi chú nội bộ
    /// </summary>
    public string? InternalNote { get; set; }

    /// <summary>
    /// Hướng dẫn giao hàng
    /// </summary>
    public string? DeliveryInstruction { get; set; }

    // ─── Audit ───────────────────────────────────────────────────────────────

    /// <summary>
    /// ID tài khoản tạo (FK to Accounts service)
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// ID tài khoản xác nhận (FK to Accounts service)
    /// </summary>
    public Guid? ConfirmedBy { get; set; }

    /// <summary>
    /// ID tài khoản hủy (FK to Accounts service)
    /// </summary>
    public Guid? CancelledBy { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
