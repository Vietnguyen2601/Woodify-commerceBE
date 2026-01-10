namespace ProductService.Application.DTOs;

public class CreateProductVersionDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public bool ArAvailable { get; set; } = false;
    public Guid? CreatedBy { get; set; }
}

public class UpdateProductVersionDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string? Currency { get; set; }
    public string? Sku { get; set; }
    public bool? ArAvailable { get; set; }
}

public class ProductVersionDto
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public bool ArAvailable { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
