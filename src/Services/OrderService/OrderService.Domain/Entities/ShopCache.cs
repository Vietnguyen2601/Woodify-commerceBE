using OrderService.Domain.Entities;

namespace OrderService.Domain.Entities;

/// <summary>
/// Cache local của Shop data từ ShopService
/// Được đồng bộ thông qua RabbitMQ events (ShopCreatedEvent, ShopUpdatedEvent)
/// </summary>
public class ShopCache
{
    public Guid ShopId { get; set; }
    public Guid OwnerAccountId { get; set; }

    // Shop Info
    public string Name { get; set; } = string.Empty;
    public string? ShopPhone { get; set; }
    public string? ShopEmail { get; set; }
    public string? ShopAddress { get; set; }

    // Pickup Address
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public string? DefaultProviderServiceCode { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
