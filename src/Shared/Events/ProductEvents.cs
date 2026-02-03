namespace Shared.Events;

/// <summary>
/// Event được publish khi ProductVersion được tạo hoặc cập nhật
/// </summary>
public class ProductVersionUpdatedEvent
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public string ProductStatus { get; set; } = "DRAFT"; // Status của ProductMaster
    public DateTime UpdatedAt { get; set; }
    public string EventType { get; set; } = "ProductVersionUpdated"; // Created, Updated, Deleted
}

/// <summary>
/// Event được publish khi ProductMaster status thay đổi
/// </summary>
public class ProductStatusChangedEvent
{
    public Guid ProductId { get; set; }
    public string Status { get; set; } = string.Empty; // DRAFT, PUBLISHED, ARCHIVED, DELETED
    public DateTime ChangedAt { get; set; }
    public string EventType { get; set; } = "ProductStatusChanged";
}
