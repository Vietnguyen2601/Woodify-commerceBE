using ProductService.Application.DTOs;
using Shared.Results;

namespace ProductService.Application.Interfaces;

/// <summary>
/// Interface cho ProductMaster Business Service
/// </summary>
public interface IProductMasterService
{
    Task<ServiceResult<ProductMasterDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ProductMasterDto>> GetByGlobalSkuAsync(string globalSku);
    Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetByShopIdAsync(Guid shopId);
    Task<ServiceResult<ProductMasterDto>> CreateAsync(CreateProductMasterDto dto);
    Task<ServiceResult<ProductMasterDto>> UpdateAsync(Guid id, UpdateProductMasterDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<ProductMasterDto>> ArchiveProductAsync(Guid id);
    Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetArchivedProductsAsync();
    Task<ServiceResult<ProductMasterDto>> PublishProductAsync(Guid id);
    Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetPublishedProductsAsync();
    Task<ServiceResult<ProductSearchResultDto>> SearchProductsAsync(ProductSearchDto searchDto);
    
    // Moderation methods
    Task<ServiceResult<ProductMasterDto>> SubmitForApprovalAsync(Guid id);
    Task<ServiceResult<ProductMasterDto>> ModerateProductAsync(Guid id, ModerateProductDto dto);
    Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetPendingApprovalProductsAsync();
    Task<ServiceResult<PendingApprovalQueueResultDto>> GetPendingApprovalQueueAsync(PendingApprovalQueueFilterDto filterDto);
    
    // Product detail with versions
    Task<ServiceResult<ProductMasterDetailDto>> GetProductDetailAsync(Guid productId, string userRole);
    Task<ServiceResult<ProductDetailListResultDto>> GetAllProductDetailsAsync(string userRole, int page = 1, int pageSize = 20, Guid? shopId = null, Guid? categoryId = null);
    
    // Submission management APIs (Issue #6)
    Task<ServiceResult<ProductMasterDto>> CancelSubmissionAsync(Guid id);
    Task<ServiceResult<SubmissionStatusDto>> GetSubmissionStatusAsync(Guid id);

    // Bestseller APIs (Issue #4)
    Task<ServiceResult<BestSellingProductsResultDto>> GetBestSellingProductsAsync(int page, int pageSize);
    Task<ServiceResult<BestSellingProductsResultDto>> GetTrendingProductsAsync(int page, int pageSize, Guid? categoryId = null);
    Task<ServiceResult<ShopBestSellingProductsResultDto>> GetBestSellingProductsByShopAsync(Guid shopId, int page, int pageSize);
}
