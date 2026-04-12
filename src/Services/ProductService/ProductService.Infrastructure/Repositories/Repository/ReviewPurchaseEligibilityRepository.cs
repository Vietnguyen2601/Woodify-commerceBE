using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories.Repository;

public class ReviewPurchaseEligibilityRepository : IReviewPurchaseEligibilityRepository
{
    private readonly ProductDbContext _context;

    public ReviewPurchaseEligibilityRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewPurchaseEligibility?> GetUnconsumedAsync(
        Guid orderId,
        Guid orderItemId,
        Guid accountId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReviewPurchaseEligibilities.AsTracking()
            .FirstOrDefaultAsync(
                e => e.OrderItemId == orderItemId
                     && e.OrderId == orderId
                     && e.AccountId == accountId
                     && e.VersionId == versionId
                     && !e.IsConsumed,
                cancellationToken);
    }

    public async Task UpsertAsync(ReviewPurchaseEligibility entity, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ReviewPurchaseEligibilities
            .AsTracking()
            .FirstOrDefaultAsync(e => e.OrderItemId == entity.OrderItemId, cancellationToken);

        if (existing == null)
        {
            await _context.ReviewPurchaseEligibilities.AddAsync(entity, cancellationToken);
        }
        else
        {
            existing.OrderId = entity.OrderId;
            existing.AccountId = entity.AccountId;
            existing.VersionId = entity.VersionId;
            existing.ProductId = entity.ProductId;
            existing.ShopId = entity.ShopId;
            existing.EligibleAt = entity.EligibleAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task PersistConsumedAsync(ReviewPurchaseEligibility eligibility, Guid reviewId, CancellationToken cancellationToken = default)
    {
        eligibility.IsConsumed = true;
        eligibility.ReviewId = reviewId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
