using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;

namespace ProductService.Application.Services;

/// <summary>
/// Applies OrderReviewEligibleEvent payloads to local eligibility rows (no HTTP to OrderService).
/// </summary>
public class OrderReviewEligibilityIngestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReviewPurchaseEligibilityRepository _eligibility;

    public OrderReviewEligibilityIngestService(
        IUnitOfWork unitOfWork,
        IReviewPurchaseEligibilityRepository eligibility)
    {
        _unitOfWork = unitOfWork;
        _eligibility = eligibility;
    }

    public async Task IngestAsync(OrderReviewEligibleEvent evt, CancellationToken cancellationToken = default)
    {
        foreach (var line in evt.Lines)
        {
            var version = await _unitOfWork.ProductVersions.GetByIdAsync(line.VersionId);
            if (version?.Product == null)
                continue;

            await _eligibility.UpsertAsync(new ReviewPurchaseEligibility
            {
                OrderItemId = line.OrderItemId,
                OrderId = evt.OrderId,
                AccountId = evt.AccountId,
                VersionId = line.VersionId,
                ProductId = version.ProductId,
                ShopId = version.Product.ShopId,
                EligibleAt = evt.EligibleAt,
                IsConsumed = false
            }, cancellationToken);
        }
    }
}
