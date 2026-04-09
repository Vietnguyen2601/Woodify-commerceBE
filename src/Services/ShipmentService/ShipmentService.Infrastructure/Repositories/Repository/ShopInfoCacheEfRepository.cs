using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Data.Context;

namespace ShipmentService.Infrastructure.Repositories.Repository;

/// <summary>shop_cache — đăng ký Scoped; HostedService tạo scope khi xử lý message.</summary>
public class ShopInfoCacheEfRepository : IShopInfoCacheRepository
{
    private readonly ShipmentDbContext _ctx;

    public ShopInfoCacheEfRepository(ShipmentDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task SaveShopInfoAsync(ShopInfoCache info)
    {
        var set = _ctx.Set<ShopCache>();
        var existing = await set.FirstOrDefaultAsync(s => s.ShopId == info.ShopId);
        if (existing != null)
        {
            existing.OwnerAccountId = info.OwnerAccountId;
            existing.Name = info.ShopName;
            existing.DefaultPickupAddress = info.DefaultPickupAddress;
            existing.DefaultProvider = info.DefaultProvider;
        }
        else
        {
            await set.AddAsync(new ShopCache
            {
                ShopId = info.ShopId,
                OwnerAccountId = info.OwnerAccountId,
                Name = info.ShopName,
                DefaultPickupAddress = info.DefaultPickupAddress,
                DefaultProvider = info.DefaultProvider
            });
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task<ShopInfoCache?> GetShopInfoAsync(Guid shopId)
    {
        var row = await _ctx.Set<ShopCache>().FirstOrDefaultAsync(s => s.ShopId == shopId);
        if (row == null)
            return null;

        return new ShopInfoCache
        {
            ShopId = row.ShopId,
            OwnerAccountId = row.OwnerAccountId,
            ShopName = row.Name,
            DefaultPickupAddress = row.DefaultPickupAddress,
            DefaultProvider = row.DefaultProvider,
            DefaultProviderServiceCode = null
        };
    }

    public async Task DeleteByShopIdAsync(Guid shopId)
    {
        var row = await _ctx.Set<ShopCache>().FirstOrDefaultAsync(s => s.ShopId == shopId);
        if (row != null)
        {
            _ctx.Set<ShopCache>().Remove(row);
            await _ctx.SaveChangesAsync();
        }
    }
}
