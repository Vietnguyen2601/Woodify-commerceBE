namespace ProductService.Domain.Entities;

/// <summary>
/// Entity Category - Bảng Category
/// Quản lý danh mục sản phẩm với cấu trúc phân cấp
/// </summary>
public class Category
{
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<ProductMaster> Products { get; set; } = new List<ProductMaster>();
}
