namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductVersion - Bảng Product_Version
/// Quản lý phiên bản sản phẩm
/// </summary>
public class ProductVersion
{
    public Guid VersionId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public bool ArAvailable { get; set; } = false;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ProductMaster? Product { get; set; }
}
