using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho ProductReview
/// </summary>
public interface IProductReviewRepository : IGenericRepository<ProductReview>
{
    Task<List<ProductReview>> GetByProductIdAsync(Guid productId);
    Task<List<ProductReview>> GetByAccountIdAsync(Guid accountId);
    Task<List<ProductReview>> GetByOrderIdAsync(Guid orderId);
    Task<List<ProductReview>> GetVisibleReviewsAsync(Guid productId);
    Task<List<ProductReview>> GetByVersionIdAsync(Guid versionId);
    Task<ProductReview?> GetByOrderItemAndAccountAsync(Guid orderItemId, Guid accountId);

    /// <summary>
    /// Recomputes product-level aggregates on ProductMaster; returns shop-level stats for publishing to ShopService.
    /// </summary>
    Task<(Guid ShopId, double? ShopAverageRating, int ShopReviewCount)?> RecomputeRatingAggregatesAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}
