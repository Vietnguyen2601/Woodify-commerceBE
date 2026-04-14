using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

public interface IReviewPurchaseEligibilityRepository
{
    Task<ReviewPurchaseEligibility?> GetUnconsumedAsync(
        Guid orderId,
        Guid orderItemId,
        Guid accountId,
        Guid versionId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(ReviewPurchaseEligibility entity, CancellationToken cancellationToken = default);

    /// <summary>Persists eligibility row already loaded as tracked.</summary>
    Task PersistConsumedAsync(ReviewPurchaseEligibility eligibility, Guid reviewId, CancellationToken cancellationToken = default);
}
