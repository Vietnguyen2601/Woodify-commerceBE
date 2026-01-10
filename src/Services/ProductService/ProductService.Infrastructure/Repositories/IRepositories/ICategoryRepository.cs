using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Category
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<List<Category>> GetRootCategoriesAsync();
    Task<List<Category>> GetSubCategoriesAsync(Guid parentCategoryId);
    Task<Category?> GetByNameAsync(string name);
    Task<List<Category>> GetActiveCategoriesAsync();
}
