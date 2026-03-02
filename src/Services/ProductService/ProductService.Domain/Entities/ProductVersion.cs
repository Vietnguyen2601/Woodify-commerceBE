namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductVersion - Bảng Product_Versions
/// Quản lý phiên bản sản phẩm với thông tin chi tiết về giá, tồn kho, kích thước
/// </summary>
public class ProductVersion
{
    public Guid VersionId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }

    public string SellerSku { get; set; } = string.Empty;

    public string? VersionName { get; set; }

    public double Price { get; set; }

    public int StockQuantity { get; set; } = 0;

    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ProductMaster? Product { get; set; }
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
