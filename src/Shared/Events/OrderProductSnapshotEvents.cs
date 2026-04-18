namespace Shared.Events;

/// <summary>
/// Line item snapshot for ProductService order mirror (order.events / order.product.snapshot).
/// </summary>
public class OrderSnapshotLineForProduct
{
    public Guid OrderItemId { get; set; }
    public Guid VersionId { get; set; }
    public int Quantity { get; set; }
    public long UnitPriceVnd { get; set; }
    public double LineTotalVnd { get; set; }
}

/// <summary>
/// Full order snapshot for ProductService (sales by version, analytics, no HTTP).
/// Published on create and on every status-changing transition (including payment completion).
/// Exchange: order.events / Routing key: order.product.snapshot
/// </summary>
public class OrderSnapshotForProductEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public long CommissionVnd { get; set; }
    public decimal CommissionRate { get; set; }
    public Guid? VoucherId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string ProviderServiceCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime SnapshotAt { get; set; }
    public List<OrderSnapshotLineForProduct> Lines { get; set; } = new();
}
