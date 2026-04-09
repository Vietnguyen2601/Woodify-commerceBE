using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

public interface IShopInfoCacheRepository
{
    Task<ShopInfoCache?> GetByShopIdAsync(Guid shopId);
    Task UpsertAsync(ShopInfoCache row);
}
