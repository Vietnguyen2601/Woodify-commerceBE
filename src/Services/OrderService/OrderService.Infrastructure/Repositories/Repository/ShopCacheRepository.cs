using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class ShopCacheRepository : GenericRepository<ShopCache>, IShopCacheRepository
{
    public ShopCacheRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<ShopCache?> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.ShopId == shopId && !s.IsDeleted);
    }

    public async Task<List<ShopCache>> GetAllActiveAsync()
    {
        return await _dbSet.Where(s => !s.IsDeleted).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid shopId)
    {
        return await _dbSet.AnyAsync(s => s.ShopId == shopId && !s.IsDeleted);
    }

    public async Task UpsertAsync(ShopCache cache)
    {
        var existing = await GetByShopIdAsync(cache.ShopId);
        if (existing != null)
        {
            // Update fields
            existing.Name = cache.Name;
            existing.ShopPhone = cache.ShopPhone;
            existing.ShopEmail = cache.ShopEmail;
            existing.ShopAddress = cache.ShopAddress;
            existing.DefaultPickupAddress = cache.DefaultPickupAddress;
            existing.DefaultProvider = cache.DefaultProvider;
            existing.DefaultProviderServiceCode = cache.DefaultProviderServiceCode;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.LastSyncedAt = DateTime.UtcNow;

            await UpdateAsync(existing);
        }
        else
        {
            cache.LastSyncedAt = DateTime.UtcNow;
            await CreateAsync(cache);
        }
    }
}
