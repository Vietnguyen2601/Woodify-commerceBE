namespace OrderService.Domain.Entities;

/// <summary>
/// Entity OrderItem - Bảng Order_Items
/// Quản lý các item trong đơn hàng
/// </summary>
public class OrderItem
{
    public Guid OrderItemId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    
    public Guid? ProductId { get; set; }
    public Guid? ProductVersionId { get; set; }
    public string? SkuCode { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public long UnitPriceCents { get; set; }
    public int Qty { get; set; } = 1;
    public long TaxCents { get; set; } = 0;
    public long LineTotalCents { get; set; }
    
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.UNFULFILLED;
    public int ReturnedQty { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Order? Order { get; set; }
}

public enum FulfillmentStatus
{
    UNFULFILLED,
    PICKED,
    SHIPPED,
    RETURNED
}
