using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Events;
using ShopService.Infrastructure.Data.Context;

namespace ShopService.Application.Consumers;

/// <summary>
/// Increments <c>shops.total_orders</c> idempotently from <see cref="OrderCreatedForShopEvent"/>.
/// </summary>
public class ShopTotalOrdersConsumer
{
    private readonly ShopDbContext _db;
    private readonly ILogger<ShopTotalOrdersConsumer> _logger;

    public ShopTotalOrdersConsumer(ShopDbContext db, ILogger<ShopTotalOrdersConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedForShopEvent evt, CancellationToken cancellationToken = default)
    {
        var alreadyCounted = await _db.ShopOrderCounterLedgers
            .AnyAsync(x => x.OrderId == evt.OrderId, cancellationToken);
        if (alreadyCounted)
            return;

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.ShopId == evt.ShopId, cancellationToken);
        if (shop == null)
            return;

        _db.ShopOrderCounterLedgers.Add(new Domain.Entities.ShopOrderCounterLedger
        {
            OrderId = evt.OrderId,
            ShopId = evt.ShopId,
            CountedAt = DateTime.UtcNow
        });

        shop.TotalOrders = Math.Max(0, shop.TotalOrders + 1);
        shop.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shop {ShopId} TotalOrders incremented to {TotalOrders}", shop.ShopId, shop.TotalOrders);
    }
}

/// <summary>
/// Maintains <c>shops.total_products</c> using ProductService events (eventual consistency).
/// </summary>
public class ShopTotalProductsConsumer
{
    private readonly ShopDbContext _db;
    private readonly ILogger<ShopTotalProductsConsumer> _logger;

    public ShopTotalProductsConsumer(ShopDbContext db, ILogger<ShopTotalProductsConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleVersionUpdatedAsync(ProductVersionUpdatedEvent evt, CancellationToken cancellationToken = default)
    {
        // We only count products that are currently PUBLISHED.
        var status = (evt.ProductStatus ?? string.Empty).Trim().ToUpperInvariant();
        var shouldCount = status == "PUBLISHED";

        var existing = await _db.ShopProductCounterLedgers
            .FirstOrDefaultAsync(x => x.ProductId == evt.ProductId, cancellationToken);

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.ShopId == evt.ShopId, cancellationToken);
        if (shop == null)
            return;

        if (shouldCount)
        {
            // Count if not already counted.
            if (existing == null)
            {
                _db.ShopProductCounterLedgers.Add(new Domain.Entities.ShopProductCounterLedger
                {
                    ProductId = evt.ProductId,
                    ShopId = evt.ShopId,
                    CountedAt = DateTime.UtcNow,
                    UncountedAt = null
                });
                shop.TotalProducts = Math.Max(0, shop.TotalProducts + 1);
            }
            else if (existing.UncountedAt != null)
            {
                existing.UncountedAt = null;
                shop.TotalProducts = Math.Max(0, shop.TotalProducts + 1);
            }
            else
            {
                return; // already counted
            }
        }
        else
        {
            // Un-count if it was counted before and is no longer published.
            if (existing == null || existing.UncountedAt != null)
                return;

            existing.UncountedAt = DateTime.UtcNow;
            shop.TotalProducts = Math.Max(0, shop.TotalProducts - 1);
        }

        shop.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Shop {ShopId} TotalProducts updated to {TotalProducts}", shop.ShopId, shop.TotalProducts);
    }

    public async Task HandleProductDeletedAsync(ProductDeletedEvent evt, CancellationToken cancellationToken = default)
    {
        var row = await _db.ShopProductCounterLedgers
            .FirstOrDefaultAsync(x => x.ProductId == evt.ProductId, cancellationToken);

        if (row == null || row.UncountedAt != null)
            return;

        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.ShopId == evt.ShopId, cancellationToken);
        if (shop == null)
            return;

        row.UncountedAt = DateTime.UtcNow;
        shop.TotalProducts = Math.Max(0, shop.TotalProducts - 1);
        shop.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Shop {ShopId} TotalProducts decremented to {TotalProducts}", shop.ShopId, shop.TotalProducts);
    }
}

