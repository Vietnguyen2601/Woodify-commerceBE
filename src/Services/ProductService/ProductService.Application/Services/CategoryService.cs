using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ServiceResult<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return ServiceResult<CategoryDto>.NotFound("Category not found");
        
        return ServiceResult<CategoryDto>.Success(category.ToDto());
    }

    public async Task<ServiceResult<CategoryDto>> GetByNameAsync(string name)
    {
        var category = await _categoryRepository.GetByNameAsync(name);
        if (category == null)
            return ServiceResult<CategoryDto>.NotFound("Category not found");
        
        return ServiceResult<CategoryDto>.Success(category.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var categoryDtos = categories.Select(c => c.ToDto());
        
        return ServiceResult<IEnumerable<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetRootCategoriesAsync();
        var categoryDtos = categories.Select(c => c.ToDto());
        
        return ServiceResult<IEnumerable<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetSubCategoriesAsync(Guid parentCategoryId)
    {
        var categories = await _categoryRepository.GetSubCategoriesAsync(parentCategoryId);
        var categoryDtos = categories.Select(c => c.ToDto());
        
        return ServiceResult<IEnumerable<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync();
        var categoryDtos = categories.Select(c => c.ToDto());
        
        return ServiceResult<IEnumerable<CategoryDto>>.Success(categoryDtos);
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        try
        {
            // Validate parent category exists if provided
            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _categoryRepository.ExistsAsync(dto.ParentCategoryId.Value);
                if (!parentExists)
                    return ServiceResult<CategoryDto>.NotFound($"Parent category with ID {dto.ParentCategoryId} not found");
            }

            var category = dto.ToModel();
            await _categoryRepository.CreateAsync(category);

            var createdCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);
            return ServiceResult<CategoryDto>.Created(createdCategory!.ToDto(), "Category created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDto>.InternalServerError($"Error creating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ServiceResult<CategoryDto>.NotFound("Category not found");

            // Prevent circular reference
            if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == id)
                return ServiceResult<CategoryDto>.BadRequest("Category cannot be its own parent");

            // Validate parent category exists if provided
            if (dto.ParentCategoryId.HasValue)
            {
                var parentExists = await _categoryRepository.ExistsAsync(dto.ParentCategoryId.Value);
                if (!parentExists)
                    return ServiceResult<CategoryDto>.NotFound($"Parent category with ID {dto.ParentCategoryId} not found");
            }

            dto.MapToUpdate(category);
            await _categoryRepository.UpdateAsync(category);
            
            var updatedCategory = await _categoryRepository.GetByIdAsync(id);
            return ServiceResult<CategoryDto>.Success(updatedCategory!.ToDto(), "Category updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CategoryDto>.InternalServerError($"Error updating category: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ServiceResult.NotFound("Category not found");

            // Check if category has subcategories
            if (category.SubCategories != null && category.SubCategories.Any())
                return ServiceResult.BadRequest("Cannot delete category with subcategories. Please delete or reassign subcategories first.");
            
            await _categoryRepository.RemoveAsync(category);
            return ServiceResult.Success("Category deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting category: {ex.Message}");
        }
    }
}
