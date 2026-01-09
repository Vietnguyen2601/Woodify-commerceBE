using ProductService.Domain.Entities;

namespace ProductService.Application.DTOs;

public class CreateProductMasterDto
{
    public Guid ShopId { get; set; }
    public string? GlobalSku { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.DRAFT;
    public bool Certified { get; set; } = false;
    public Guid? CurrentVersionId { get; set; }
}

public class UpdateProductMasterDto
{
    public string? GlobalSku { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? Certified { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public decimal? AvgRating { get; set; }
    public int? ReviewCount { get; set; }
}

public class ProductMasterDto
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public string? GlobalSku { get; set; }
    public ProductStatus Status { get; set; }
    public bool Certified { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
