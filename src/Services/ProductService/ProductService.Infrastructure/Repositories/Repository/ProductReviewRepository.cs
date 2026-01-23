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
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
    }

    public async Task<List<ProductReview>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<ProductReview>> GetVerifiedReviewsAsync(Guid productId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId && r.IsVerified)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductReview?> GetByOrderAndAccountAsync(Guid orderId, Guid accountId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.OrderId == orderId && r.AccountId == accountId);
    }

    public override async Task<List<ProductReview>> GetAllAsync()
    {
        return await _dbSet
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(r => r.ReviewId == id);
    }
}
