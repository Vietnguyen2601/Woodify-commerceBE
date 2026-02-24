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
    Task<ProductReview?> GetByOrderAndAccountAsync(Guid orderId, Guid accountId);
}
