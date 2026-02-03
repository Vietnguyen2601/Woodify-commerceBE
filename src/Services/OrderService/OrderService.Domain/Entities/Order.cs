namespace OrderService.Domain.Entities;

/// <summary>
/// Entity Order - Bảng Orders
/// Quản lý đơn hàng
/// </summary>
public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public string OrderCode { get; set; } = string.Empty;
    
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    
    public string Currency { get; set; } = "VND";
    public long SubtotalCents { get; set; }
    public long ShippingFeeCents { get; set; } = 0;
    public long TotalAmountCents { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ShippingAddress { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    PENDING,
    CONFIRMED,
    SHIPPED,
    DELIVERED,
    CANCELLED,
    RETURNED,
    REFUNDED
}
