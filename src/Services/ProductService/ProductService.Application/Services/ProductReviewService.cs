using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using Shared.Results;

namespace ProductService.Application.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepository _productReviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductReviewService(IProductReviewRepository productReviewRepository, IUnitOfWork unitOfWork)
    {
        _productReviewRepository = productReviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<ProductReviewDto>> GetByIdAsync(Guid id)
    {
        var review = await _productReviewRepository.GetByIdAsync(id);
        if (review == null)
            return ServiceResult<ProductReviewDto>.NotFound("Review not found");
        
        return ServiceResult<ProductReviewDto>.Success(review.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetAllAsync()
    {
        var reviews = await _productReviewRepository.GetAllAsync();
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByProductIdAsync(Guid productId)
    {
        var reviews = await _productReviewRepository.GetByProductIdAsync(productId);
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByAccountIdAsync(Guid accountId)
    {
        var reviews = await _productReviewRepository.GetByAccountIdAsync(accountId);
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByOrderIdAsync(Guid orderId)
    {
        var reviews = await _productReviewRepository.GetByOrderIdAsync(orderId);
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetVisibleReviewsAsync(Guid productId)
    {
        var reviews = await _productReviewRepository.GetVisibleReviewsAsync(productId);
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByVersionIdAsync(Guid versionId)
    {
        var reviews = await _productReviewRepository.GetByVersionIdAsync(versionId);
        var reviewDtos = reviews.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(reviewDtos);
    }

    public async Task<ServiceResult<ProductReviewDto>> CreateAsync(CreateProductReviewDto dto)
    {
        try
        {
            // Validate Product exists
            var productExists = await _unitOfWork.ProductMasters.ExistsAsync(dto.ProductId);
            if (!productExists)
                return ServiceResult<ProductReviewDto>.NotFound("Product not found");

            // Check if user already reviewed this order
            var existingReview = await _productReviewRepository.GetByOrderAndAccountAsync(dto.OrderId, dto.AccountId);
            if (existingReview != null)
                return ServiceResult<ProductReviewDto>.BadRequest("You have already reviewed this order");

            var review = dto.ToModel();
            await _productReviewRepository.CreateAsync(review);

            return ServiceResult<ProductReviewDto>.Created(review.ToDto(), "Review created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error creating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> UpdateAsync(Guid id, UpdateProductReviewDto dto)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult<ProductReviewDto>.NotFound("Review not found");

            dto.MapToUpdate(review);
            await _productReviewRepository.UpdateAsync(review);
            
            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(updatedReview!.ToDto(), "Review updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error updating review: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult.NotFound("Review not found");
            
            await _productReviewRepository.RemoveAsync(review);
            return ServiceResult.Success("Review deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> HideReviewAsync(Guid id, Guid hiddenBy)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult<ProductReviewDto>.NotFound("Review not found");

            review.IsVisible = false;
            review.HiddenBy = hiddenBy;
            review.HiddenAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            
            await _productReviewRepository.UpdateAsync(review);
            
            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(updatedReview!.ToDto(), "Review hidden successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error hiding review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> UnhideReviewAsync(Guid id)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult<ProductReviewDto>.NotFound("Review not found");

            review.IsVisible = true;
            review.HiddenBy = null;
            review.HiddenAt = null;
            review.UpdatedAt = DateTime.UtcNow;
            
            await _productReviewRepository.UpdateAsync(review);
            
            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(updatedReview!.ToDto(), "Review unhidden successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error unhiding review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> AddShopResponseAsync(Guid id, ShopResponseDto dto)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult<ProductReviewDto>.NotFound("Review not found");

            review.ShopResponse = dto.ShopResponse;
            review.ShopResponseAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            
            await _productReviewRepository.UpdateAsync(review);
            
            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(updatedReview!.ToDto(), "Shop response added successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error adding shop response: {ex.Message}");
        }
    }
}
