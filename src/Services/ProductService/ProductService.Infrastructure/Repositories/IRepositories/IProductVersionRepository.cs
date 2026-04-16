using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductVersion
/// </summary>
public interface IProductVersionRepository : IGenericRepository<ProductVersion>
{
    Task<List<ProductVersion>> GetByProductIdAsync(Guid productId);
    Task<ProductVersion?> GetBySellerSkuAsync(string sellerSku);
    Task<ProductVersion?> GetLatestVersionByProductIdAsync(Guid productId);
    Task<List<ProductVersion>> GetInactiveVersionsAsync();
    Task<List<ProductVersion>> GetActiveVersionsAsync();
    Task<ProductVersion?> GetDefaultVersionByProductIdAsync(Guid productId);
}
