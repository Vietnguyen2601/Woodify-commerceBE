namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductVersion - Bảng Product_Version
/// Quản lý phiên bản sản phẩm với thông tin chi tiết về giá, tồn kho, kích thước
/// </summary>
public class ProductVersion
{
    public Guid VersionId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }

    public string SellerSku { get; set; } = string.Empty;

    public int VersionNumber { get; set; } = 1;
    public string? VersionName { get; set; }

    public long PriceCents { get; set; }
    public long? BasePriceCents { get; set; }

    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5;
    public bool AllowBackorder { get; set; } = false;

    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public long? VolumeCm3 { get; set; }

    public string? BulkyType { get; set; } // NORMAL, BULKY, SUPER_BULKY
    public bool IsFragile { get; set; } = false;
    public bool RequiresSpecialHandling { get; set; } = false;

    // Bảo hành
    public int WarrantyMonths { get; set; } = 12;
    public string? WarrantyTerms { get; set; }

    // Combo - giảm giá combo
    public bool IsBundle { get; set; } = false;
    public long BundleDiscountCents { get; set; } = 0;

    public string? PrimaryImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ProductMaster? Product { get; set; }
}
