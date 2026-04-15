using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class CategoryCacheRepository : GenericRepository<CategoryCache>, ICategoryCacheRepository
{
    public CategoryCacheRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<CategoryCache?> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.CategoryId == categoryId && !c.IsDeleted);
    }

    public async Task<List<CategoryCache>> GetAllActiveAsync()
    {
        return await _dbSet.Where(c => c.IsActive && !c.IsDeleted).ToListAsync();
    }

    public async Task UpsertAsync(CategoryCache cache)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(c => c.CategoryId == cache.CategoryId);
        if (existing != null)
        {
            existing.Name = cache.Name;
            existing.Description = cache.Description;
            existing.ParentCategoryId = cache.ParentCategoryId;
            existing.Level = cache.Level;
            existing.IsActive = cache.IsActive;
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

    public async Task SoftDeleteAsync(Guid categoryId)
    {
        var category = await GetByCategoryIdAsync(categoryId);
        if (category != null)
        {
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(category);
        }
    }
}
