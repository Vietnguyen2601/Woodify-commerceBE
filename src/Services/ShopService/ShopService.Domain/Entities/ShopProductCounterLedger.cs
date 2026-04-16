namespace ShopService.Domain.Entities;

/// <summary>
/// Idempotency ledger: one row per product that has been counted into <c>shops.total_products</c>.
/// Decrement is applied once when <c>product.deleted</c> is received.
/// </summary>
public class ShopProductCounterLedger
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime CountedAt { get; set; }

    public DateTime? UncountedAt { get; set; }
}

