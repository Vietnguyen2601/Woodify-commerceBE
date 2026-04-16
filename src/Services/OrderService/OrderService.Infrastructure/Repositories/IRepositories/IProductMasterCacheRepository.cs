using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductMasterCache
/// </summary>
public interface IProductMasterCacheRepository : IGenericRepository<ProductMasterCache>
{
    Task<ProductMasterCache?> GetByProductIdAsync(Guid productId);
    Task<List<ProductMasterCache>> GetByCategoryIdAsync(Guid categoryId);
    Task<List<ProductMasterCache>> GetByShopIdAsync(Guid shopId);
    Task<List<ProductMasterCache>> GetAllActivePublishedAsync();
    Task UpsertAsync(ProductMasterCache cache);
    Task SoftDeleteAsync(Guid productId);
}
