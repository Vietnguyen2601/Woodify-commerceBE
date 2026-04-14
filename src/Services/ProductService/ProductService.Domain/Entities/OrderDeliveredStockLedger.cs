namespace ProductService.Domain.Entities;

/// <summary>
/// Idempotency: one row per order after stock was decremented for delivery (order.events / order.delivered.stock).
/// </summary>
public class OrderDeliveredStockLedger
{
    public Guid OrderId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
