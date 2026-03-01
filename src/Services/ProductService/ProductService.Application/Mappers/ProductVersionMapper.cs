using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class ProductVersionMapper
{
    public static ProductVersionDto ToDto(this ProductVersion version)
    {
        if (version == null) throw new ArgumentNullException(nameof(version), "ProductVersion cannot be null");
        
        return new ProductVersionDto
        {
            VersionId = version.VersionId,
            ProductId = version.ProductId,
            SellerSku = version.SellerSku,
            VersionName = version.VersionName,
            Price = version.Price,
            StockQuantity = version.StockQuantity,
            WeightGrams = version.WeightGrams,
            LengthCm = version.LengthCm,
            WidthCm = version.WidthCm,
            HeightCm = version.HeightCm,
            IsActive = version.IsActive,
            CreatedAt = version.CreatedAt,
            UpdatedAt = version.UpdatedAt
        };
    }

    public static ProductVersion ToModel(this CreateProductVersionDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateProductVersionDto cannot be null.");
        }

        return new ProductVersion
        {
            ProductId = dto.ProductId,
            SellerSku = dto.SellerSku,
            VersionName = dto.VersionName,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            WeightGrams = dto.WeightGrams,
            LengthCm = dto.LengthCm,
            WidthCm = dto.WidthCm,
            HeightCm = dto.HeightCm,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void MapToUpdate(this UpdateProductVersionDto dto, ProductVersion version)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "UpdateProductVersionDto cannot be null.");
        }
        if (version == null)
        {
            throw new ArgumentNullException(nameof(version), "ProductVersion cannot be null.");
        }

        if (dto.SellerSku != null)
            version.SellerSku = dto.SellerSku;
        
        if (dto.VersionName != null)
            version.VersionName = dto.VersionName;
        
        if (dto.Price.HasValue)
            version.Price = dto.Price.Value;
        
        if (dto.StockQuantity.HasValue)
            version.StockQuantity = dto.StockQuantity.Value;
        
        if (dto.WeightGrams.HasValue)
            version.WeightGrams = dto.WeightGrams.Value;
        
        if (dto.LengthCm.HasValue)
            version.LengthCm = dto.LengthCm.Value;
        
        if (dto.WidthCm.HasValue)
            version.WidthCm = dto.WidthCm.Value;
        
        if (dto.HeightCm.HasValue)
            version.HeightCm = dto.HeightCm.Value;
        
        if (dto.IsActive.HasValue)
            version.IsActive = dto.IsActive.Value;

        version.UpdatedAt = DateTime.UtcNow;
    }
}
