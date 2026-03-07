using ShopService.Application.DTOs;
using ShopService.Domain.Entities;

namespace ShopService.Application.Mappers;

public static class ShopMapper
{
    public static ShopDto ToDto(this Shop shop)
    {
        return new ShopDto
        {
            ShopId = shop.ShopId,
            OwnerId = shop.OwnerAccountId,
            Name = shop.Name,
            Description = shop.Description,
            LogoUrl = shop.LogoUrl,
            CoverImageUrl = shop.CoverImageUrl,
            DefaultPickupAddress = shop.DefaultPickupAddress,
            DefaultProvider = shop.DefaultProvider,
            Rating = shop.Rating,
            ReviewCount = shop.ReviewCount,
            TotalProducts = shop.TotalProducts,
            TotalOrders = shop.TotalOrders,
            Status = shop.Status.ToString(),
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt
        };
    }

    public static IEnumerable<ShopDto> ToDto(this IEnumerable<Shop> shops)
    {
        return shops.Select(s => s.ToDto());
    }

    public static Shop ToModel(this CreateShopDto dto)
    {
        return new Shop
        {
            OwnerAccountId = dto.OwnerAccountId,
            Name = dto.Name,
            Description = dto.Description,
            LogoUrl = dto.LogoUrl,
            CoverImageUrl = dto.CoverImageUrl,
            DefaultPickupAddress = dto.DefaultPickupAddress,
            DefaultProvider = dto.DefaultProvider
        };
    }

    public static void MapToUpdate(this Shop shop, UpdateShopDto dto)
    {
        shop.Name = dto.Name;
        shop.Description = dto.Description;
        shop.LogoUrl = dto.LogoUrl;
        shop.CoverImageUrl = dto.CoverImageUrl;
        shop.DefaultPickupAddress = dto.DefaultPickupAddress;
        shop.DefaultProvider = dto.DefaultProvider;
        shop.UpdatedAt = DateTime.UtcNow;
    }

    public static void MapToShopInfo(this Shop shop, UpdateShopInfoDto dto)
    {
        if (dto.Name != null) shop.Name = dto.Name;
        if (dto.Description != null) shop.Description = dto.Description;
        if (dto.LogoUrl != null) shop.LogoUrl = dto.LogoUrl;
        if (dto.CoverImageUrl != null) shop.CoverImageUrl = dto.CoverImageUrl;
        if (dto.DefaultPickupAddress != null) shop.DefaultPickupAddress = dto.DefaultPickupAddress;
        if (dto.DefaultProvider.HasValue) shop.DefaultProvider = dto.DefaultProvider;
        shop.UpdatedAt = DateTime.UtcNow;
    }
}
