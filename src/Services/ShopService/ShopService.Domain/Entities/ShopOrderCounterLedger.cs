namespace ShopService.Domain.Entities;

/// <summary>
/// Idempotency ledger: one row per order that has been counted into <c>shops.total_orders</c>.
/// </summary>
public class ShopOrderCounterLedger
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime CountedAt { get; set; }
}

