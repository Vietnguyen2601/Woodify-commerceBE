using ProductService.Application.DTOs;
using Shared.Results;

namespace ProductService.Application.Interfaces;

/// <summary>
/// Interface cho ProductReview Business Service
/// </summary>
public interface IProductReviewService
{
    Task<ServiceResult<ProductReviewDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByProductIdAsync(Guid productId);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByAccountIdAsync(Guid accountId);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByOrderIdAsync(Guid orderId);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetVisibleReviewsAsync(Guid productId);
    Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByVersionIdAsync(Guid versionId);
    Task<ServiceResult<ProductReviewDto>> CreateAsync(CreateProductReviewDto dto);
    Task<ServiceResult<ProductReviewDto>> UpdateAsync(Guid id, UpdateProductReviewDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<ProductReviewDto>> HideReviewAsync(Guid id);
    Task<ServiceResult<ProductReviewDto>> UnhideReviewAsync(Guid id);
    Task<ServiceResult<ProductReviewDto>> AddShopResponseAsync(Guid id, ShopResponseDto dto);
}
