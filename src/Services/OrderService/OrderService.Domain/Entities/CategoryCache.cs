namespace OrderService.Domain.Entities;

/// <summary>
/// Cache local của Category data từ ProductService
/// Được đồng bộ thông qua RabbitMQ events
/// </summary>
public class CategoryCache
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; } = true;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Sync tracking
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
