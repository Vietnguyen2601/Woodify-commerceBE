using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category), "Category cannot be null");
        
        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            IsActive = category.IsActive,
            SubCategoriesCount = category.SubCategories?.Count ?? 0
        };
    }

    public static Category ToModel(this CreateCategoryDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateCategoryDto cannot be null.");
        }

        return new Category
        {
            ParentCategoryId = dto.ParentCategoryId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void MapToUpdate(this UpdateCategoryDto dto, Category category)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "UpdateCategoryDto cannot be null.");
        }
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category), "Category cannot be null.");
        }

        if (dto.ParentCategoryId.HasValue)
            category.ParentCategoryId = dto.ParentCategoryId;
        
        if (dto.Name != null)
            category.Name = dto.Name;
        
        if (dto.Description != null)
            category.Description = dto.Description;
        
        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;
    }
}
