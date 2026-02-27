namespace ProductService.Application.DTOs;

public class CreateProductReviewDto
{
    public Guid VersionId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
}

public class UpdateProductReviewDto
{
    public int? Rating { get; set; }
    public string? Content { get; set; }
}

public class ShopResponseDto
{
    public string ShopResponse { get; set; } = string.Empty;
}

public class ProductReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid VersionId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public bool IsVisible { get; set; }
    public string? ShopResponse { get; set; }
    public DateTime? ShopResponseAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
