using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductVersionCache
/// </summary>
public interface IProductVersionCacheRepository : IGenericRepository<ProductVersionCache>
{
    Task<ProductVersionCache?> GetByVersionIdAsync(Guid versionId);
    Task<List<ProductVersionCache>> GetByProductIdAsync(Guid productId);
    Task UpsertAsync(ProductVersionCache cache);
    Task UpdateProductStatusAsync(Guid productId, string status);
}
