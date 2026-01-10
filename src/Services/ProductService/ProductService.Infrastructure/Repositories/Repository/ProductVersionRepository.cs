using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.Base;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories;

public class ProductVersionRepository : GenericRepository<ProductVersion>, IProductVersionRepository
{
    public ProductVersionRepository(ProductDbContext context) : base(context)
    {
    }

    public override async Task<ProductVersion?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.VersionId == id);
    }

    public async Task<List<ProductVersion>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(v => v.Product)
            .Where(v => v.ProductId == productId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProductVersion?> GetBySkuAsync(string sku)
    {
        return await _dbSet
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Sku == sku);
    }

    public async Task<ProductVersion?> GetLatestVersionByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(v => v.Product)
            .Where(v => v.ProductId == productId)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public override async Task<List<ProductVersion>> GetAllAsync()
    {
        return await _dbSet
            .Include(v => v.Product)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(v => v.VersionId == id);
    }
}
