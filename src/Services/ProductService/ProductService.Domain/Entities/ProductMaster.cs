namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductMaster - Bảng Product_Master
/// Quản lý thông tin master của sản phẩm
/// </summary>
public class ProductMaster
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? GlobalSku { get; set; }

    public string? ImgUrl { get; set; }
    public string? Description { get; set; }

    public bool ArAvailable { get; set; } = false;
    public string? ArModelUrl { get; set; }

    public decimal AvgRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public int SoldCount { get; set; } = 0;

    public ProductStatus Status { get; set; } = ProductStatus.DRAFT;

    // Moderation fields
    public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.PENDING;
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? ModerationNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation property
    public virtual Category? Category { get; set; }
}

/// <summary>
/// Enum cho trạng thái sản phẩm
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
/// Enum cho trạng thái duyệt sản phẩm
/// </summary>
public enum ModerationStatus
{
    PENDING,
    APPROVED,
    REJECTED
}
