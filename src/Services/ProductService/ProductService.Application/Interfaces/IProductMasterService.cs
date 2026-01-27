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
}
