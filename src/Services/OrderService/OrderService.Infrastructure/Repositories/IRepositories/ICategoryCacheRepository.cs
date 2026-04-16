using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho CategoryCache
/// </summary>
public interface ICategoryCacheRepository : IGenericRepository<CategoryCache>
{
    Task<CategoryCache?> GetByCategoryIdAsync(Guid categoryId);
    Task<List<CategoryCache>> GetAllActiveAsync();
    Task UpsertAsync(CategoryCache cache);
    Task SoftDeleteAsync(Guid categoryId);
}
