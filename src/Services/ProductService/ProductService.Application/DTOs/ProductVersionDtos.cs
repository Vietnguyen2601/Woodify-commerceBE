namespace ProductService.Application.DTOs;

public class CreateProductVersionDto
{
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
}

public class UpdateProductVersionDto
{
    public string? SellerSku { get; set; }
    public string? VersionName { get; set; }
    public double? Price { get; set; }
    public int? StockQuantity { get; set; }
    public int? WeightGrams { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public bool? IsActive { get; set; }
}

public class ProductVersionDto
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string SellerSku { get; set; } = string.Empty;
    public string? VersionName { get; set; }
    public double Price { get; set; }
    public int StockQuantity { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
