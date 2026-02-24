using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class ProductReviewMapper
{
    public static ProductReviewDto ToDto(this ProductReview review)
    {
        if (review == null) throw new ArgumentNullException(nameof(review), "ProductReview cannot be null");
        
        return new ProductReviewDto
        {
            ReviewId = review.ReviewId,
            ProductId = review.ProductId,
            VersionId = review.VersionId,
            OrderId = review.OrderId,
            AccountId = review.AccountId,
            Rating = review.Rating,
            Content = review.Content,
            IsVisible = review.IsVisible,
            HiddenBy = review.HiddenBy,
            HiddenAt = review.HiddenAt,
            ShopResponse = review.ShopResponse,
            ShopResponseAt = review.ShopResponseAt,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public static ProductReview ToModel(this CreateProductReviewDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateProductReviewDto cannot be null.");
        }

        return new ProductReview
        {
            ProductId = dto.ProductId,
            VersionId = dto.VersionId,
            OrderId = dto.OrderId,
            AccountId = dto.AccountId,
            Rating = dto.Rating,
            Content = dto.Content,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void MapToUpdate(this UpdateProductReviewDto dto, ProductReview review)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "UpdateProductReviewDto cannot be null.");
        }

        if (review == null)
        {
            throw new ArgumentNullException(nameof(review), "ProductReview cannot be null.");
        }

        if (dto.Rating.HasValue)
            review.Rating = dto.Rating.Value;

        if (dto.Content != null)
            review.Content = dto.Content;

        review.UpdatedAt = DateTime.UtcNow;
    }
}
