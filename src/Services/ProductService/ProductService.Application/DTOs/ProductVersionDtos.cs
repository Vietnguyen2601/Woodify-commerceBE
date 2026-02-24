namespace ProductService.Application.DTOs;

public class CreateProductVersionDto
{
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
    public string? BulkyType { get; set; }
    public bool IsFragile { get; set; } = false;
    public bool RequiresSpecialHandling { get; set; } = false;
    public int WarrantyMonths { get; set; } = 12;
    public string? WarrantyTerms { get; set; }
    public bool IsBundle { get; set; } = false;
    public long BundleDiscountCents { get; set; } = 0;
    public string? PrimaryImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class UpdateProductVersionDto
{
    public string? SellerSku { get; set; }
    public int? VersionNumber { get; set; }
    public string? VersionName { get; set; }
    public long? PriceCents { get; set; }
    public long? BasePriceCents { get; set; }
    public int? StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool? AllowBackorder { get; set; }
    public int? WeightGrams { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public long? VolumeCm3 { get; set; }
    public string? BulkyType { get; set; }
    public bool? IsFragile { get; set; }
    public bool? RequiresSpecialHandling { get; set; }
    public int? WarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }
    public bool? IsBundle { get; set; }
    public long? BundleDiscountCents { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
}

public class ProductVersionDto
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string SellerSku { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string? VersionName { get; set; }
    public long PriceCents { get; set; }
    public long? BasePriceCents { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool AllowBackorder { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public long? VolumeCm3 { get; set; }
    public string? BulkyType { get; set; }
    public bool IsFragile { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public int WarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }
    public bool IsBundle { get; set; }
    public long BundleDiscountCents { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
