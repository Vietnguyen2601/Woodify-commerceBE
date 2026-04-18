using Microsoft.EntityFrameworkCore;
using ShopService.Application.Interfaces;
using ShopService.Infrastructure.Data.Context;

namespace ShopService.Application.Queries;

public class ShopReadModelTotalsQuery : IShopReadModelTotalsQuery
{
    private readonly ShopDbContext _context;

    public ShopReadModelTotalsQuery(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<Guid, (int TotalProducts, int TotalOrders)>> GetTotalsByShopIdsAsync(
        IReadOnlyList<Guid> shopIds,
        CancellationToken cancellationToken = default)
    {
        if (shopIds.Count == 0)
            return new Dictionary<Guid, (int, int)>();

        var distinctIds = shopIds.Distinct().ToList();

        var productCounts = await _context.ProductMasterReplicas
            .AsNoTracking()
            .Where(r => distinctIds.Contains(r.ShopId) && !r.IsDeleted)
            .GroupBy(r => r.ShopId)
            .Select(g => new { ShopId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.ShopId, x => x.Cnt, cancellationToken);

        var orderCounts = await _context.OrderMetricsSnapshots
            .AsNoTracking()
            .Where(o => distinctIds.Contains(o.ShopId))
            .GroupBy(o => o.ShopId)
            .Select(g => new { ShopId = g.Key, Cnt = g.Count() })
            .ToDictionaryAsync(x => x.ShopId, x => x.Cnt, cancellationToken);

        return distinctIds.ToDictionary(
            id => id,
            id => (productCounts.GetValueOrDefault(id), orderCounts.GetValueOrDefault(id)));
    }
}
