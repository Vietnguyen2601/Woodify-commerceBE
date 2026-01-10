using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductMaster
/// </summary>
public interface IProductMasterRepository : IGenericRepository<ProductMaster>
{
    Task<ProductMaster?> GetByGlobalSkuAsync(string globalSku);
    Task<List<ProductMaster>> GetByShopIdAsync(Guid shopId);
    Task<List<ProductMaster>> GetByStatusAsync(ProductStatus status);
}
