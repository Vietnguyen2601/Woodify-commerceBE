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
            OrderId = review.OrderId,
            AccountId = review.AccountId,
            Rating = review.Rating,
            Title = review.Title,
            Content = review.Content,
            IsVerified = review.IsVerified,
            HelpfulCount = review.HelpfulCount,
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
            OrderId = dto.OrderId,
            AccountId = dto.AccountId,
            Rating = dto.Rating,
            Title = dto.Title,
            Content = dto.Content,
            IsVerified = dto.IsVerified,
            HelpfulCount = 0,
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

        if (dto.Title != null)
            review.Title = dto.Title;

        if (dto.Content != null)
            review.Content = dto.Content;

        if (dto.IsVerified.HasValue)
            review.IsVerified = dto.IsVerified.Value;

        if (dto.HelpfulCount.HasValue)
            review.HelpfulCount = dto.HelpfulCount.Value;

        review.UpdatedAt = DateTime.UtcNow;
    }
}
