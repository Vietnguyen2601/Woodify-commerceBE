using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class ProductVersionCacheRepository : GenericRepository<ProductVersionCache>, IProductVersionCacheRepository
{
    public ProductVersionCacheRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<ProductVersionCache?> GetByVersionIdAsync(Guid versionId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.VersionId == versionId);
    }

    public async Task<List<ProductVersionCache>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet.Where(p => p.ProductId == productId).ToListAsync();
    }

    public async Task UpsertAsync(ProductVersionCache cache)
    {
        var existing = await GetByVersionIdAsync(cache.VersionId);
        if (existing != null)
        {
            existing.ProductId = cache.ProductId;
            existing.Title = cache.Title;
            existing.Description = cache.Description;
            existing.PriceCents = cache.PriceCents;
            existing.Currency = cache.Currency;
            existing.Sku = cache.Sku;
            existing.ProductStatus = cache.ProductStatus;
            existing.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(existing);
        }
        else
        {
            cache.LastUpdated = DateTime.UtcNow;
            await CreateAsync(cache);
        }
    }

    public async Task UpdateProductStatusAsync(Guid productId, string status)
    {
        var versions = await GetByProductIdAsync(productId);
        foreach (var version in versions)
        {
            version.ProductStatus = status;
            version.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(version);
        }
    }
}
