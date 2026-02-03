namespace OrderService.Domain.Entities;

/// <summary>
/// Entity CartItem - Bảng Cart_Items
/// Quản lý các sản phẩm trong giỏ hàng
/// </summary>
public class CartItem
{
    public Guid CartItemId { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid ProductVersionId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long UnitPriceCents { get; set; }
    public int Qty { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual Cart? Cart { get; set; }
}
