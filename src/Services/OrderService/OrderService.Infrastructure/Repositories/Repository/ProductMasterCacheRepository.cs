using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class ProductMasterCacheRepository : GenericRepository<ProductMasterCache>, IProductMasterCacheRepository
{
    public ProductMasterCacheRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<ProductMasterCache?> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);
    }

    public async Task<List<ProductMasterCache>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _dbSet.Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.Status == "PUBLISHED").ToListAsync();
    }

    public async Task<List<ProductMasterCache>> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet.Where(p => p.ShopId == shopId && !p.IsDeleted && p.Status == "PUBLISHED").ToListAsync();
    }

    public async Task<List<ProductMasterCache>> GetAllActivePublishedAsync()
    {
        return await _dbSet.Where(p => !p.IsDeleted && p.Status == "PUBLISHED").ToListAsync();
    }

    public async Task UpsertAsync(ProductMasterCache cache)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(p => p.ProductId == cache.ProductId);
        if (existing != null)
        {
            existing.ShopId = cache.ShopId;
            existing.CategoryId = cache.CategoryId;
            existing.Name = cache.Name;
            existing.Description = cache.Description;
            existing.Status = cache.Status;
            existing.ModerationStatus = cache.ModerationStatus;
            existing.HasVersions = cache.HasVersions;
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(existing);
        }
        else
        {
            cache.LastUpdated = DateTime.UtcNow;
            await CreateAsync(cache);
        }
    }

    public async Task SoftDeleteAsync(Guid productId)
    {
        var product = await GetByProductIdAsync(productId);
        if (product != null)
        {
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.Status = "DELETED";
            product.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(product);
        }
    }
}
