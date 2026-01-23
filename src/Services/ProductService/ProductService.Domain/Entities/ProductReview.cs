namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductReview - Bảng Product_Review
/// Quản lý đánh giá sản phẩm từ người dùng
/// </summary>
public class ProductReview
{
    public Guid ReviewId { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsVerified { get; set; } = false;
    public int HelpfulCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ProductMaster? Product { get; set; }
}
