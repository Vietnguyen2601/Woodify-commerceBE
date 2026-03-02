using ShipmentService.Domain.Enums;

namespace ShipmentService.Domain.Entities;

/// <summary>
/// Entity ProviderService - Bảng Provider_Services
/// Quản lý các dịch vụ vận chuyển từ các đối tác (GHN, GHTK, ViettelPost, ...)
/// </summary>
public class ProviderService
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public Guid ServiceId { get; set; } = Guid.NewGuid();

    // ─── Thông tin Provider ───────────────────────────────────────────────────

    /// <summary>
    /// Mã provider (ví dụ: GHN, GHTK, VIETTELPOST)
    /// </summary>
    public string ProviderCode { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của provider (ví dụ: Giao Hàng Nhanh)
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// URL logo của provider
    /// </summary>
    public string? ProviderLogoUrl { get; set; }

    // ─── Thông tin Dịch vụ ───────────────────────────────────────────────────

    /// <summary>
    /// Mã dịch vụ cụ thể (ví dụ: GHN_EXPRESS, GHN_ECONOMY)
    /// </summary>
    public string? ServiceCode { get; set; }

    /// <summary>
    /// Tên dịch vụ cụ thể (ví dụ: Giao hàng nhanh, Giao hàng tiết kiệm)
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    // ─── Tốc độ & Thời gian ─────────────────────────────────────────────────

    /// <summary>
    /// Mức tốc độ dịch vụ: ECONOMY, STANDARD, EXPRESS, SUPER_EXPRESS
    /// </summary>
    public SpeedLevel? SpeedLevel { get; set; }

    /// <summary>
    /// Số ngày giao hàng ước tính
    /// </summary>
    public int? EstimatedDeliveryDays { get; set; }

    // ─── Cấu hình & Giới hạn ─────────────────────────────────────────────────

    /// <summary>
    /// Giới hạn dịch vụ (JSON string: khối lượng tối đa, kích thước tối đa, v.v.)
    /// </summary>
    public string? Limitations { get; set; }

    /// <summary>
    /// Cấu hình vùng phủ sóng (JSON string)
    /// </summary>
    public string? ZoneConfig { get; set; }

    /// <summary>
    /// Quy tắc tính phí (JSON string)
    /// </summary>
    public string? PricingRules { get; set; }

    /// <summary>
    /// Cấu hình phí nền tảng (JSON string)
    /// </summary>
    public string? PlatformFeeConfig { get; set; }

    // ─── Trạng thái & Ưu tiên ────────────────────────────────────────────────

    /// <summary>
    /// Dịch vụ đang hoạt động
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thứ tự ưu tiên hiển thị (số nhỏ hơn = ưu tiên cao hơn)
    /// </summary>
    public int PriorityOrder { get; set; } = 0;

    // ─── Navigation Properties ────────────────────────────────────────────────

    /// <summary>
    /// Danh sách shipment sử dụng dịch vụ này
    /// </summary>
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    // ─── Audit ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
