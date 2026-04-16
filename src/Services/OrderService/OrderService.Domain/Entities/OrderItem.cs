namespace OrderService.Domain.Entities;

/// <summary>
/// Entity OrderItem - Bảng Order_Items
/// Quản lý các item trong đơn hàng
/// </summary>
public class OrderItem
{
    public Guid OrderItemId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid VersionId { get; set; }
    
    // Tiền từng món
    public long UnitPriceVnd { get; set; }
    public int Quantity { get; set; } = 1;
    
    public long DiscountVnd { get; set; } = 0;
    public double TaxVnd { get; set; } = 0;
    
    // Vì mỗi món hàng có thể tới từ nhiều đơn vị khác nhau nên ship sẽ khác nhau
    public Guid? ShipmentId { get; set; }
    
    // Tổng tiền cuối
    public double LineTotalVnd { get; set; }
    
    public FulfillmentStatus Status { get; set; } = FulfillmentStatus.UNFULFILLED;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Order? Order { get; set; }
}

public enum FulfillmentStatus
{
    UNFULFILLED,
    PICKED,
    PACKED,
    SHIPPED,
    DELIVERED,
    RETURNED,
    CANCELLED
}
