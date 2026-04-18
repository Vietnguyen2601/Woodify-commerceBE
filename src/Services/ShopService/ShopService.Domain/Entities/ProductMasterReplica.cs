namespace ShopService.Domain.Entities;

/// <summary>
/// Read model đồng bộ từ ProductService (RabbitMQ: ProductMasterCreated/Updated, ProductDeleted).
/// </summary>
public class ProductMasterReplica
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? ModerationStatus { get; set; }
    public bool HasVersions { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
