namespace ShipmentService.Domain.Entities;

/// <summary>
/// Entity ProviderService - Bảng Provider_Services
/// Quản lý các gói dịch vụ vận chuyển của từng nhà cung cấp
/// </summary>
public class ProviderService
{
    public Guid ServiceId { get; set; } = Guid.NewGuid();

    public Guid ProviderId { get; set; }

    /// <summary>ECO, STD, EXP, SUP</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Tiết kiệm, Nhanh, Hỏa tốc, ...</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ECONOMY, STANDARD, EXPRESS, ...</summary>
    public string? SpeedLevel { get; set; }

    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Hệ số nhân phí vận chuyển</summary>
    public double? MultiplierFee { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ShippingProvider? ShippingProvider { get; set; }
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
