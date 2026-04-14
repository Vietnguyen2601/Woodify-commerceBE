using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> GetOrdersByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.AccountId == accountId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByShopIdAsync(Guid shopId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.ShopId == shopId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public override async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }

    public async Task<List<Order>> GetByIdsAsync(IEnumerable<Guid> orderIds)
    {
        var ids = orderIds.ToList();
        if (ids.Count == 0) return new List<Order>();

        return await _dbSet
            .Where(o => ids.Contains(o.OrderId))
            .ToListAsync();
    }

    public async Task<(List<Order> Items, int Total)> GetAllPagedAsync(int page, int pageSize, string? status, Guid? shopId, Guid? accountId)
    {
        var query = _dbSet.Include(o => o.OrderItems).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status.ToUpper(), out var parsedStatus))
            query = query.Where(o => o.Status == parsedStatus);

        if (shopId.HasValue)
            query = query.Where(o => o.ShopId == shopId.Value);

        if (accountId.HasValue)
            query = query.Where(o => o.AccountId == accountId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<(Guid ProductId, long UnitsSold)>> GetTopSellingProductMasterAggregatesAsync(
        int productLimit,
        Guid? shopId,
        CancellationToken cancellationToken = default)
    {
        // Only DELIVERED orders count toward "sold". Payment-only COMPLETED is excluded.
        var versionSales =
            from oi in _context.OrderItems
            join o in _context.Orders on oi.OrderId equals o.OrderId
            where o.Status == OrderStatus.DELIVERED && (!shopId.HasValue || o.ShopId == shopId.Value)
            group oi by oi.VersionId
            into g
            select new
            {
                VersionId = g.Key,
                UnitsSold = g.Sum(x => (long)x.Quantity)
            };

        var versionRows = await versionSales.ToListAsync(cancellationToken);
        if (versionRows.Count == 0)
            return new List<(Guid ProductId, long UnitsSold)>();

        var versionIds = versionRows.Select(v => v.VersionId).Distinct().ToList();
        var cacheLinks = await _context.ProductVersionCaches
            .AsNoTracking()
            .Where(c => versionIds.Contains(c.VersionId) && !c.IsDeleted)
            .Select(c => new { c.VersionId, c.ProductId })
            .ToListAsync(cancellationToken);

        var versionToProduct = cacheLinks.ToDictionary(x => x.VersionId, x => x.ProductId);

        return versionRows
            .Where(v => versionToProduct.ContainsKey(v.VersionId))
            .GroupBy(v => versionToProduct[v.VersionId])
            .Select(g => (ProductId: g.Key, UnitsSold: g.Sum(x => x.UnitsSold)))
            .OrderByDescending(x => x.UnitsSold)
            .Take(productLimit)
            .ToList();
    }
}
