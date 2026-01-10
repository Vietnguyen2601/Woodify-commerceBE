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
            GlobalSku = product.GlobalSku,
            Status = product.Status,
            Certified = product.Certified,
            CurrentVersionId = product.CurrentVersionId,
            AvgRating = product.AvgRating,
            ReviewCount = product.ReviewCount,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
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
            GlobalSku = dto.GlobalSku,
            Status = dto.Status,
            Certified = dto.Certified,
            CurrentVersionId = dto.CurrentVersionId,
            AvgRating = 0,
            ReviewCount = 0,
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

        if (dto.GlobalSku != null)
            product.GlobalSku = dto.GlobalSku;
        
        if (dto.Status.HasValue)
            product.Status = dto.Status.Value;
        
        if (dto.Certified.HasValue)
            product.Certified = dto.Certified.Value;
        
        if (dto.CurrentVersionId.HasValue)
            product.CurrentVersionId = dto.CurrentVersionId;
        
        if (dto.AvgRating.HasValue)
            product.AvgRating = dto.AvgRating.Value;
        
        if (dto.ReviewCount.HasValue)
            product.ReviewCount = dto.ReviewCount.Value;

        product.UpdatedAt = DateTime.UtcNow;
    }
}
