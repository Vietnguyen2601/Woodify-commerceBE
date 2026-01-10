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
            Title = version.Title,
            Description = version.Description,
            PriceCents = version.PriceCents,
            Currency = version.Currency,
            Sku = version.Sku,
            ArAvailable = version.ArAvailable,
            CreatedBy = version.CreatedBy,
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
            Title = dto.Title,
            Description = dto.Description,
            PriceCents = dto.PriceCents,
            Currency = dto.Currency,
            Sku = dto.Sku,
            ArAvailable = dto.ArAvailable,
            CreatedBy = dto.CreatedBy,
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

        if (dto.Title != null)
            version.Title = dto.Title;
        
        if (dto.Description != null)
            version.Description = dto.Description;
        
        if (dto.PriceCents.HasValue)
            version.PriceCents = dto.PriceCents;
        
        if (dto.Currency != null)
            version.Currency = dto.Currency;
        
        if (dto.Sku != null)
            version.Sku = dto.Sku;
        
        if (dto.ArAvailable.HasValue)
            version.ArAvailable = dto.ArAvailable.Value;

        version.UpdatedAt = DateTime.UtcNow;
    }
}
