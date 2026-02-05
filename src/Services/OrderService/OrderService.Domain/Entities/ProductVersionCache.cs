using OrderService.Domain.Entities;

namespace OrderService.Domain.Entities;

/// <summary>
/// Cache local của Product Version data từ ProductService
/// Được đồng bộ thông qua RabbitMQ events
/// </summary>
public class ProductVersionCache
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public string ProductStatus { get; set; } = "DRAFT"; // Status của ProductMaster
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
