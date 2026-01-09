using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.Base;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories;

public class ProductMasterRepository : GenericRepository<ProductMaster>, IProductMasterRepository
{
    public ProductMasterRepository(ProductDbContext context) : base(context)
    {
    }

    public override async Task<ProductMaster?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<ProductMaster?> GetByGlobalSkuAsync(string globalSku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.GlobalSku == globalSku);
    }

    public async Task<List<ProductMaster>> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet
            .Where(p => p.ShopId == shopId)
            .ToListAsync();
    }

    public async Task<List<ProductMaster>> GetByStatusAsync(ProductStatus status)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public override async Task<List<ProductMaster>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(p => p.ProductId == id);
    }
}
