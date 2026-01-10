using ProductService.Application.DTOs;
using Shared.Results;

namespace ProductService.Application.Interfaces;

/// <summary>
/// Interface cho Category Business Service
/// </summary>
public interface ICategoryService
{
    Task<ServiceResult<CategoryDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<CategoryDto>> GetByNameAsync(string name);
    Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<CategoryDto>>> GetRootCategoriesAsync();
    Task<ServiceResult<IEnumerable<CategoryDto>>> GetSubCategoriesAsync(Guid parentCategoryId);
    Task<ServiceResult<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync();
    Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<ServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}
