using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class ProductMasterMapper
{
    public static ProductMasterDto ToDto(this ProductMaster product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product), "ProductMaster cannot be null");
        
        return new ProductMasterDto
        {
            ProductId = product.ProductId,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            Name = product.Name,
            GlobalSku = product.GlobalSku,
            Description = product.Description,
            Status = product.Status,
            ModerationStatus = product.ModerationStatus,
            ModeratedAt = product.ModeratedAt,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            PublishedAt = product.PublishedAt
        };
    }

    public static ProductMaster ToModel(this CreateProductMasterDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateProductMasterDto cannot be null.");
        }

        return new ProductMaster
        {
            ShopId = dto.ShopId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            GlobalSku = null,
            Description = dto.Description,
            Status = ProductStatus.DRAFT,
            ModerationStatus = ModerationStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void MapToUpdate(this UpdateProductMasterDto dto, ProductMaster product)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "UpdateProductMasterDto cannot be null.");
        }
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product), "ProductMaster cannot be null.");
        }

        if (dto.CategoryId.HasValue)
            product.CategoryId = dto.CategoryId.Value;

        if (dto.Name != null)
            product.Name = dto.Name;

        if (dto.Description != null)
            product.Description = dto.Description;

        product.UpdatedAt = DateTime.UtcNow;
    }

    public static void MapToModerate(this ModerateProductDto dto, ProductMaster product)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "ModerateProductDto cannot be null.");
        }
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product), "ProductMaster cannot be null.");
        }

        product.ModerationStatus = dto.ModerationStatus;
        product.ModeratedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        // Auto update status based on moderation
        if (dto.ModerationStatus == ModerationStatus.APPROVED)
        {
            product.Status = ProductStatus.APPROVED;
        }
        else if (dto.ModerationStatus == ModerationStatus.REJECTED)
        {
            product.Status = ProductStatus.REJECTED;
        }
    }
}
