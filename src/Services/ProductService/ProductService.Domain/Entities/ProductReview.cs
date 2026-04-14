namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ProductReview - Bảng Product_Reviews
/// Quản lý đánh giá sản phẩm từ người dùng
/// </summary>
public class ProductReview
{
    public Guid ReviewId { get; set; } = Guid.NewGuid();
    public Guid VersionId { get; set; }

    /// <summary>Denormalized for filters and aggregates.</summary>
    public Guid ProductId { get; set; }

    public Guid OrderId { get; set; }

    /// <summary>Matches OrderService order_items.order_item_id.</summary>
    public Guid OrderItemId { get; set; }

    public Guid AccountId { get; set; }

    public int Rating { get; set; } // 1-5
    public string? Content { get; set; }

    public bool IsVisible { get; set; } = true;  // false = bị ẩn khỏi public

    // Shop reply
    public string? ShopResponse { get; set; }
    public DateTime? ShopResponseAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ProductVersion? Version { get; set; }
}
