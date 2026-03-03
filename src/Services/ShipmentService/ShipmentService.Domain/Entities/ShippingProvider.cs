namespace ShipmentService.Domain.Entities;

/// <summary>
/// Entity ShippingProvider - Bảng Shipping_Providers
/// Quản lý nhà cung cấp dịch vụ vận chuyển
/// </summary>
public class ShippingProvider
{
    public Guid ProviderId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
}
