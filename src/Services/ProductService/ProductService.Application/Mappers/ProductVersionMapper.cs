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
            VersionNumber = version.VersionNumber,
            VersionName = version.VersionName,
            PriceCents = version.PriceCents,
            BasePriceCents = version.BasePriceCents,
            StockQuantity = version.StockQuantity,
            LowStockThreshold = version.LowStockThreshold,
            AllowBackorder = version.AllowBackorder,
            WeightGrams = version.WeightGrams,
            LengthCm = version.LengthCm,
            WidthCm = version.WidthCm,
            HeightCm = version.HeightCm,
            VolumeCm3 = version.VolumeCm3,
            BulkyType = version.BulkyType,
            IsFragile = version.IsFragile,
            RequiresSpecialHandling = version.RequiresSpecialHandling,
            WarrantyMonths = version.WarrantyMonths,
            WarrantyTerms = version.WarrantyTerms,
            IsBundle = version.IsBundle,
            BundleDiscountCents = version.BundleDiscountCents,
            PrimaryImageUrl = version.PrimaryImageUrl,
            IsActive = version.IsActive,
            IsDefault = version.IsDefault,
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
            VersionNumber = dto.VersionNumber,
            VersionName = dto.VersionName,
            PriceCents = dto.PriceCents,
            BasePriceCents = dto.BasePriceCents,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            AllowBackorder = dto.AllowBackorder,
            WeightGrams = dto.WeightGrams,
            LengthCm = dto.LengthCm,
            WidthCm = dto.WidthCm,
            HeightCm = dto.HeightCm,
            VolumeCm3 = dto.VolumeCm3,
            BulkyType = dto.BulkyType,
            IsFragile = dto.IsFragile,
            RequiresSpecialHandling = dto.RequiresSpecialHandling,
            WarrantyMonths = dto.WarrantyMonths,
            WarrantyTerms = dto.WarrantyTerms,
            IsBundle = dto.IsBundle,
            BundleDiscountCents = dto.BundleDiscountCents,
            PrimaryImageUrl = dto.PrimaryImageUrl,
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault,
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
        
        if (dto.VersionNumber.HasValue)
            version.VersionNumber = dto.VersionNumber.Value;
        
        if (dto.VersionName != null)
            version.VersionName = dto.VersionName;
        
        if (dto.PriceCents.HasValue)
            version.PriceCents = dto.PriceCents.Value;
        
        if (dto.BasePriceCents.HasValue)
            version.BasePriceCents = dto.BasePriceCents;
        
        if (dto.StockQuantity.HasValue)
            version.StockQuantity = dto.StockQuantity.Value;
        
        if (dto.LowStockThreshold.HasValue)
            version.LowStockThreshold = dto.LowStockThreshold.Value;
        
        if (dto.AllowBackorder.HasValue)
            version.AllowBackorder = dto.AllowBackorder.Value;
        
        if (dto.WeightGrams.HasValue)
            version.WeightGrams = dto.WeightGrams.Value;
        
        if (dto.LengthCm.HasValue)
            version.LengthCm = dto.LengthCm.Value;
        
        if (dto.WidthCm.HasValue)
            version.WidthCm = dto.WidthCm.Value;
        
        if (dto.HeightCm.HasValue)
            version.HeightCm = dto.HeightCm.Value;
        
        if (dto.VolumeCm3.HasValue)
            version.VolumeCm3 = dto.VolumeCm3;
        
        if (dto.BulkyType != null)
            version.BulkyType = dto.BulkyType;
        
        if (dto.IsFragile.HasValue)
            version.IsFragile = dto.IsFragile.Value;
        
        if (dto.RequiresSpecialHandling.HasValue)
            version.RequiresSpecialHandling = dto.RequiresSpecialHandling.Value;
        
        if (dto.WarrantyMonths.HasValue)
            version.WarrantyMonths = dto.WarrantyMonths.Value;
        
        if (dto.WarrantyTerms != null)
            version.WarrantyTerms = dto.WarrantyTerms;
        
        if (dto.IsBundle.HasValue)
            version.IsBundle = dto.IsBundle.Value;
        
        if (dto.BundleDiscountCents.HasValue)
            version.BundleDiscountCents = dto.BundleDiscountCents.Value;
        
        if (dto.PrimaryImageUrl != null)
            version.PrimaryImageUrl = dto.PrimaryImageUrl;
        
        if (dto.IsActive.HasValue)
            version.IsActive = dto.IsActive.Value;
        
        if (dto.IsDefault.HasValue)
            version.IsDefault = dto.IsDefault.Value;

        version.UpdatedAt = DateTime.UtcNow;
    }
}
