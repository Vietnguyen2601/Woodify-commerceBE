namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductMaster - Bảng Product_Master
/// Quản lý thông tin master của sản phẩm
/// </summary>
public class ProductMaster
{
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public string? GlobalSku { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.DRAFT;
    public bool Certified { get; set; } = false;
    public Guid? CurrentVersionId { get; set; }
    public decimal AvgRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Enum cho trạng thái sản phẩm
/// </summary>
public enum ProductStatus
{
    DRAFT,
    PUBLISHED,
    ARCHIVED,
    DELETED
}
