namespace OrderService.Domain.Entities;

/// <summary>
/// Cache local của ProductMaster data từ ProductService
/// Được đồng bộ thông qua RabbitMQ events
/// </summary>
public class ProductMasterCache
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT, PUBLISHED, ARCHIVED, DELETED
    public string? ModerationStatus { get; set; }
    public bool HasVersions { get; set; } = false;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Sync tracking
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
