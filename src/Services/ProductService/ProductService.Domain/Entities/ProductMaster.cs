namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductMaster - Bảng Product_Master
/// Quản lý thông tin master của sản ph��m
/// </summary>
public class ProductMaster
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? GlobalSku { get; set; }

    public string? Description { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.DRAFT;

    // Moderation fields
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.PENDING;
    public DateTime? ModeratedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>1–5 average from visible reviews; null if none.</summary>
    public double? AverageRating { get; set; }

    /// <summary>Visible review count for this product master.</summary>
    public int ReviewCount { get; set; }

    /// <summary>Total units sold (from order line items by version). Updated via order snapshot events.</summary>
    public int Sales { get; set; }

    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<ProductVersion> Versions { get; set; } = new List<ProductVersion>();
}

/// <summary>
/// Enum cho trạng thái sản ph��m
/// </summary>
public enum ProductStatus
{
    DRAFT,
    PENDING_APPROVAL,
    APPROVED,
    PUBLISHED,
    ARCHIVED,
    REJECTED,
    DELETED
}

/// <summary>
/// Enum cho trạng thái duyệt sản ph��m
/// </summary>
public enum ModerationStatus
{
    PENDING,
    APPROVED,
    REJECTED
}
