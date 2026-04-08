using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ShopCache
/// </summary>
public interface IShopCacheRepository : IGenericRepository<ShopCache>
{
    Task<ShopCache?> GetByShopIdAsync(Guid shopId);
    Task<List<ShopCache>> GetAllActiveAsync();
    Task<bool> ExistsAsync(Guid shopId);
    Task UpsertAsync(ShopCache cache);
}
