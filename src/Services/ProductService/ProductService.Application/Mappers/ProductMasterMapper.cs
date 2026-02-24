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
            ImgUrl = product.ImgUrl,
            Description = product.Description,
            ArAvailable = product.ArAvailable,
            ArModelUrl = product.ArModelUrl,
            AvgRating = product.AvgRating,
            ReviewCount = product.ReviewCount,
            SoldCount = product.SoldCount,
            Status = product.Status,
            ModerationStatus = product.ModerationStatus,
            ModeratedBy = product.ModeratedBy,
            ModeratedAt = product.ModeratedAt,
            RejectionReason = product.RejectionReason,
            ModerationNotes = product.ModerationNotes,
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
            Description = dto.Description,
            ImgUrl = dto.ImgUrl,
            ArAvailable = dto.ArAvailable,
            ArModelUrl = dto.ArModelUrl,
            GlobalSku = null, // Will be generated later
            Status = ProductStatus.DRAFT,
            ModerationStatus = ModerationStatus.PENDING,
            AvgRating = 0,
            ReviewCount = 0,
            SoldCount = 0,
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

        if (dto.GlobalSku != null)
            product.GlobalSku = dto.GlobalSku;
        
        if (dto.ImgUrl != null)
            product.ImgUrl = dto.ImgUrl;

        if (dto.Description != null)
            product.Description = dto.Description;

        if (dto.ArAvailable.HasValue)
            product.ArAvailable = dto.ArAvailable.Value;

        if (dto.ArModelUrl != null)
            product.ArModelUrl = dto.ArModelUrl;
        
        if (dto.Status.HasValue)
            product.Status = dto.Status.Value;
        
        if (dto.AvgRating.HasValue)
            product.AvgRating = dto.AvgRating.Value;
        
        if (dto.ReviewCount.HasValue)
            product.ReviewCount = dto.ReviewCount.Value;

        if (dto.SoldCount.HasValue)
            product.SoldCount = dto.SoldCount.Value;

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
        product.ModeratedBy = dto.ModeratedBy;
        product.ModeratedAt = DateTime.UtcNow;
        product.RejectionReason = dto.RejectionReason;
        product.ModerationNotes = dto.ModerationNotes;
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
