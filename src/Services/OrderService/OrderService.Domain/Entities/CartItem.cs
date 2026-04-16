namespace OrderService.Domain.Entities;

/// <summary>
/// Entity CartItem - Bảng Cart_Items
/// Quản lý các sản phẩm trong giỏ hàng
/// </summary>
public class CartItem
{
    public Guid CartItemId { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid VersionId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; } = 1;
    public double Price { get; set; }

    // Navigation property
    public virtual Cart? Cart { get; set; }
}
