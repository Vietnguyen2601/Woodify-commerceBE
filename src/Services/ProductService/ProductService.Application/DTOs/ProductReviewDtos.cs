namespace ProductService.Application.DTOs;

public class CreateProductReviewDto
{
    public Guid VersionId { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>Order line id from OrderService — must match eligibility ingested via <c>order.review_eligible</c>.</summary>
    public Guid OrderItemId { get; set; }

    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }

    /// <summary>Optional image URLs stored as <c>REVIEW</c> images for this review after create (max 5 total).</summary>
    public List<string>? ImageUrls { get; set; }
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
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public bool IsVisible { get; set; }
    public string? ShopResponse { get; set; }
    public DateTime? ShopResponseAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<string> ImageUrls { get; set; } = new();
}
