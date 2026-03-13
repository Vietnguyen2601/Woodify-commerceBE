using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;
using ProductService.Domain.Parameters;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductMaster
/// </summary>
public interface IProductMasterRepository : IGenericRepository<ProductMaster>
{
    Task<ProductMaster?> GetByGlobalSkuAsync(string globalSku);
    Task<List<ProductMaster>> GetByShopIdAsync(Guid shopId);
    Task<List<ProductMaster>> GetByStatusAsync(ProductStatus status);
    Task<(List<ProductMaster> Products, int TotalCount)> SearchAsync(ProductSearchParameters searchParams);
    Task<(List<ProductMaster> Products, int TotalCount)> GetPendingApprovalQueueAsync(
        Guid? categoryId = null, 
        Guid? shopId = null, 
        DateTime? submittedFrom = null, 
        DateTime? submittedTo = null,
        int page = 1,
        int pageSize = 20);
}
