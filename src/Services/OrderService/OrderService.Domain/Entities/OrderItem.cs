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
    
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string SellerSku { get; set; } = string.Empty;
    
    public long UnitPriceCents { get; set; }
    public int Quantity { get; set; } = 1;
    
    public long DiscountCents { get; set; } = 0;
    public long TaxCents { get; set; } = 0;
    
    public Guid? ShipmentId { get; set; }
    
    public long LineTotalCents { get; set; }
    
    public FulfillmentStatus Status { get; set; } = FulfillmentStatus.UNFULFILLED;
    
    public int ReturnedQuantity { get; set; } = 0;
    public long RefundedAmountCents { get; set; } = 0;
    
    public string? ShippingInfo { get; set; }
    
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
