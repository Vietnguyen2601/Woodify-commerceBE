namespace ProductService.Application.DTOs;

public class CreateProductReviewDto
{
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsVerified { get; set; } = false;
}

public class UpdateProductReviewDto
{
    public int? Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsVerified { get; set; }
    public int? HelpfulCount { get; set; }
}

public class ProductReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsVerified { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
