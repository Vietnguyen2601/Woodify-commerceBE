using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductVersion
/// </summary>
public interface IProductVersionRepository : IGenericRepository<ProductVersion>
{
    Task<List<ProductVersion>> GetByProductIdAsync(Guid productId);
    Task<ProductVersion?> GetBySkuAsync(string sku);
    Task<ProductVersion?> GetLatestVersionByProductIdAsync(Guid productId);
}
