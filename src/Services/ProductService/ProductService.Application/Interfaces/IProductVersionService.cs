using ProductService.Application.DTOs;
using Shared.Results;

namespace ProductService.Application.Interfaces;

/// <summary>
/// Interface cho ProductVersion Business Service
/// </summary>
public interface IProductVersionService
{
    Task<ServiceResult<ProductVersionDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ProductVersionDto>> GetBySellerSkuAsync(string sellerSku);
    Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetByProductIdAsync(Guid productId);
    Task<ServiceResult<ProductVersionDto>> GetLatestVersionByProductIdAsync(Guid productId);
    Task<ServiceResult<ProductVersionDto>> GetDefaultVersionByProductIdAsync(Guid productId);
    Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetInactiveVersionsAsync();
    Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetActiveVersionsAsync();
    Task<ServiceResult<ProductVersionDto>> CreateAsync(CreateProductVersionDto dto);
    Task<ServiceResult<ProductVersionDto>> UpdateAsync(Guid id, UpdateProductVersionDto dto);
    Task<ServiceResult> DeactivateAsync(Guid id);
    Task<ServiceResult> ActivateAsync(Guid id);
    Task<ServiceResult> SetAsDefaultAsync(Guid id);
}
