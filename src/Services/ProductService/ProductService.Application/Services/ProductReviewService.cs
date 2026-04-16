using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Results;

namespace ProductService.Application.Services;

public class ProductReviewService : IProductReviewService
{
    private const int MaxReviewImagesAtCreate = 5;

    private readonly IProductReviewRepository _productReviewRepository;
    private readonly IReviewPurchaseEligibilityRepository _eligibilityRepository;
    private readonly IImageUrlRepository _imageUrlRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductEventPublisher _eventPublisher;

    public ProductReviewService(
        IProductReviewRepository productReviewRepository,
        IReviewPurchaseEligibilityRepository eligibilityRepository,
        IImageUrlRepository imageUrlRepository,
        IUnitOfWork unitOfWork,
        ProductEventPublisher eventPublisher)
    {
        _productReviewRepository = productReviewRepository;
        _eligibilityRepository = eligibilityRepository;
        _imageUrlRepository = imageUrlRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    private void PublishShopReviewStatsIfNeeded((Guid ShopId, double? ShopAverageRating, int ShopReviewCount)? sync)
    {
        if (sync == null)
            return;
        _eventPublisher.PublishShopReviewStatsUpdated(new ShopReviewStatsUpdatedEvent
        {
            ShopId = sync.Value.ShopId,
            AverageRating = sync.Value.ShopAverageRating,
            ReviewCount = sync.Value.ShopReviewCount,
            UpdatedAt = DateTime.UtcNow
        });
    }

    private async Task<ProductReviewDto> ToDtoWithImagesAsync(ProductReview review)
    {
        var imgs = await _imageUrlRepository.GetByTypeAndReferenceAsync("REVIEW", review.ReviewId);
        return review.ToDto(imgs.Select(i => i.OriginalUrl).ToList());
    }

    private async Task<List<ProductReviewDto>> ToDtosWithImagesAsync(IReadOnlyList<ProductReview> reviews)
    {
        if (reviews.Count == 0)
            return new List<ProductReviewDto>();

        var batch = await _imageUrlRepository.GetImagesBatchAsync("REVIEW", reviews.Select(r => r.ReviewId));
        return reviews.Select(r =>
        {
            if (!batch.TryGetValue(r.ReviewId, out var list))
                return r.ToDto(Array.Empty<string>());
            return r.ToDto(list.Select(i => i.OriginalUrl).ToList());
        }).ToList();
    }

    public async Task<ServiceResult<ProductReviewDto>> GetByIdAsync(Guid id)
    {
        var review = await _productReviewRepository.GetByIdAsync(id);
        if (review == null)
            return ServiceResult<ProductReviewDto>.NotFound("Review not found");

        return ServiceResult<ProductReviewDto>.Success(await ToDtoWithImagesAsync(review));
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetAllAsync()
    {
        var reviews = await _productReviewRepository.GetAllAsync();
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByProductIdAsync(Guid productId)
    {
        var reviews = await _productReviewRepository.GetByProductIdAsync(productId);
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByAccountIdAsync(Guid accountId)
    {
        var reviews = await _productReviewRepository.GetByAccountIdAsync(accountId);
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByOrderIdAsync(Guid orderId)
    {
        var reviews = await _productReviewRepository.GetByOrderIdAsync(orderId);
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetVisibleReviewsAsync(Guid productId)
    {
        var reviews = await _productReviewRepository.GetVisibleReviewsAsync(productId);
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductReviewDto>>> GetByVersionIdAsync(Guid versionId)
    {
        var reviews = await _productReviewRepository.GetByVersionIdAsync(versionId);
        var dtos = await ToDtosWithImagesAsync(reviews);
        return ServiceResult<IEnumerable<ProductReviewDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ProductReviewDto>> CreateAsync(CreateProductReviewDto dto)
    {
        try
        {
            if (dto.Rating is < 1 or > 5)
                return ServiceResult<ProductReviewDto>.BadRequest("Rating must be between 1 and 5");

            if (dto.OrderItemId == Guid.Empty)
                return ServiceResult<ProductReviewDto>.BadRequest("OrderItemId is required");

            var imageUrls = dto.ImageUrls?.Where(u => !string.IsNullOrWhiteSpace(u)).ToList() ?? new List<string>();
            if (imageUrls.Count > MaxReviewImagesAtCreate)
                return ServiceResult<ProductReviewDto>.BadRequest(
                    $"At most {MaxReviewImagesAtCreate} review images can be submitted at create.");

            var eligibility = await _eligibilityRepository.GetUnconsumedAsync(
                dto.OrderId, dto.OrderItemId, dto.AccountId, dto.VersionId);
            if (eligibility == null)
                return ServiceResult<ProductReviewDto>.BadRequest(
                    "This purchase line is not eligible for a review yet, or it was already reviewed.");

            var versionExists = await _unitOfWork.ProductVersions.ExistsAsync(dto.VersionId);
            if (!versionExists)
                return ServiceResult<ProductReviewDto>.NotFound("Product version not found");

            var duplicate = await _productReviewRepository.GetByOrderItemAndAccountAsync(dto.OrderItemId, dto.AccountId);
            if (duplicate != null)
                return ServiceResult<ProductReviewDto>.BadRequest("You have already reviewed this order line");

            var version = await _unitOfWork.ProductVersions.GetByIdAsync(dto.VersionId);
            if (version != null && version.ProductId != eligibility.ProductId)
                return ServiceResult<ProductReviewDto>.BadRequest("Version does not match eligible product");

            var review = dto.ToModel();
            review.ProductId = eligibility.ProductId;

            (Guid ShopId, double? ShopAverageRating, int ShopReviewCount)? shopSync = null;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _productReviewRepository.CreateAsync(review);
                await _eligibilityRepository.PersistConsumedAsync(eligibility, review.ReviewId);

                if (imageUrls.Count > 0)
                {
                    var entities = new List<ImageUrl>();
                    for (var i = 0; i < imageUrls.Count; i++)
                    {
                        entities.Add(new ImageUrl
                        {
                            ImageId = Guid.NewGuid(),
                            ImageType = "REVIEW",
                            ReferenceId = review.ReviewId,
                            SortOrder = i,
                            OriginalUrl = imageUrls[i].Trim(),
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    await _imageUrlRepository.BulkCreateAsync(entities);
                }

                shopSync = await _productReviewRepository.RecomputeRatingAggregatesAsync(review.ProductId);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            PublishShopReviewStatsIfNeeded(shopSync);

            var created = await _productReviewRepository.GetByIdAsync(review.ReviewId);
            return ServiceResult<ProductReviewDto>.Created(
                await ToDtoWithImagesAsync(created!),
                "Review created successfully");
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

            var productId = review.ProductId;
            var oldRating = review.Rating;

            dto.MapToUpdate(review);
            await _productReviewRepository.UpdateAsync(review);

            if (dto.Rating.HasValue && dto.Rating.Value != oldRating)
            {
                var sync = await _productReviewRepository.RecomputeRatingAggregatesAsync(productId);
                PublishShopReviewStatsIfNeeded(sync);
            }

            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(
                await ToDtoWithImagesAsync(updatedReview!),
                "Review updated successfully");
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

            var productId = review.ProductId;

            await _productReviewRepository.RemoveAsync(review);
            var delSync = await _productReviewRepository.RecomputeRatingAggregatesAsync(productId);
            PublishShopReviewStatsIfNeeded(delSync);

            return ServiceResult.Success("Review deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting review: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductReviewDto>> HideReviewAsync(Guid id)
    {
        try
        {
            var review = await _productReviewRepository.GetByIdAsync(id);
            if (review == null)
                return ServiceResult<ProductReviewDto>.NotFound("Review not found");

            var productId = review.ProductId;

            review.IsVisible = false;
            review.UpdatedAt = DateTime.UtcNow;

            await _productReviewRepository.UpdateAsync(review);
            var hideSync = await _productReviewRepository.RecomputeRatingAggregatesAsync(productId);
            PublishShopReviewStatsIfNeeded(hideSync);

            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(
                await ToDtoWithImagesAsync(updatedReview!),
                "Review hidden successfully");
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

            var productId = review.ProductId;

            review.IsVisible = true;
            review.UpdatedAt = DateTime.UtcNow;

            await _productReviewRepository.UpdateAsync(review);
            var unhideSync = await _productReviewRepository.RecomputeRatingAggregatesAsync(productId);
            PublishShopReviewStatsIfNeeded(unhideSync);

            var updatedReview = await _productReviewRepository.GetByIdAsync(id);
            return ServiceResult<ProductReviewDto>.Success(
                await ToDtoWithImagesAsync(updatedReview!),
                "Review unhidden successfully");
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
            return ServiceResult<ProductReviewDto>.Success(
                await ToDtoWithImagesAsync(updatedReview!),
                "Shop response added successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductReviewDto>.InternalServerError($"Error adding shop response: {ex.Message}");
        }
    }
}
