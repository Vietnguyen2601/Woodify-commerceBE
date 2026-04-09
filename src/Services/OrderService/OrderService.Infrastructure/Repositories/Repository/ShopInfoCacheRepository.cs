using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.IRepositories;

namespace OrderService.Infrastructure.Repositories.Repository;

public class ShopInfoCacheRepository : IShopInfoCacheRepository
{
    private readonly OrderDbContext _context;
    private readonly DbSet<ShopInfoCache> _dbSet;

    public ShopInfoCacheRepository(OrderDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ShopInfoCache>();
    }

    public async Task<ShopInfoCache?> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.ShopId == shopId);
    }

    public async Task UpsertAsync(ShopInfoCache row)
    {
        var existing = await GetByShopIdAsync(row.ShopId);
        if (existing != null)
        {
            existing.OwnerAccountId = row.OwnerAccountId;
            existing.Name = row.Name;
            existing.DefaultPickupAddress = row.DefaultPickupAddress;
            existing.DefaultProvider = row.DefaultProvider;
            existing.DefaultProviderServiceCode = row.DefaultProviderServiceCode;
            existing.UpdatedAt = row.UpdatedAt != default ? row.UpdatedAt : DateTime.UtcNow;
        }
        else
        {
            row.UpdatedAt = row.UpdatedAt == default ? DateTime.UtcNow : row.UpdatedAt;
            await _dbSet.AddAsync(row);
        }

        await _context.SaveChangesAsync();
    }
}
