using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.Base;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories.Repository;

public class ProductReviewRepository : GenericRepository<ProductReview>, IProductReviewRepository
{
    public ProductReviewRepository(ProductDbContext context) : base(context)
    {
    }

    public override async Task<ProductReview?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.Version)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
    }

    public async Task<List<ProductReview>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetVisibleReviewsAsync(Guid productId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .Where(r => r.ProductId == productId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetByVersionIdAsync(Guid versionId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .Where(r => r.VersionId == versionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductReview?> GetByOrderItemAndAccountAsync(Guid orderItemId, Guid accountId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .FirstOrDefaultAsync(r => r.OrderItemId == orderItemId && r.AccountId == accountId);
    }

    public async Task<(Guid ShopId, double? ShopAverageRating, int ShopReviewCount)?> RecomputeRatingAggregatesAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var master = await _context.ProductMasters.AsTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (master == null)
            return null;

        var visibleForProduct = await _context.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsVisible)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        var pCount = visibleForProduct.Count;
        master.AverageRating = pCount == 0 ? null : visibleForProduct.Average(x => (double)x);
        master.ReviewCount = pCount;
        master.UpdatedAt = DateTime.UtcNow;

        var shopId = master.ShopId;
        var shopProductIds = await _context.ProductMasters.AsNoTracking()
            .Where(p => p.ShopId == shopId)
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken);

        var shopRatings = await _context.ProductReviews.AsNoTracking()
            .Where(r => r.IsVisible && shopProductIds.Contains(r.ProductId))
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        var sCount = shopRatings.Count;
        double? sAvg = sCount == 0 ? null : shopRatings.Average(x => (double)x);

        await _context.SaveChangesAsync(cancellationToken);

        return (shopId, sAvg, sCount);
    }

    public override async Task<List<ProductReview>> GetAllAsync()
    {
        return await _dbSet
            .Include(r => r.Version)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(r => r.ReviewId == id);
    }
}
