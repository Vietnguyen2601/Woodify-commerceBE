namespace OrderService.Domain.Entities;

/// <summary>
/// Entity Cart - Bảng Carts
/// Quản lý giỏ hàng của người dùng
/// </summary>
public class Cart
{
    public Guid CartId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
