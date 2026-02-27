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
        // Get reviews by querying through Version -> Product relationship
        return await _dbSet
            .Include(r => r.Version)
                .ThenInclude(v => v.Product)
            .Where(r => r.Version.ProductId == productId)
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
                .ThenInclude(v => v.Product)
            .Where(r => r.Version.ProductId == productId && r.IsVisible)
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

    public async Task<ProductReview?> GetByOrderAndAccountAsync(Guid orderId, Guid accountId)
    {
        return await _dbSet
            .Include(r => r.Version)
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.AccountId == accountId);
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
