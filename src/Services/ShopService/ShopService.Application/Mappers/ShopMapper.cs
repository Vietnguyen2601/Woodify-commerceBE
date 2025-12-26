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
            ShopName = shop.Name,
            Description = shop.Description,
            OwnerId = shop.OwnerAccountId,
            IsActive = shop.Status == Domain.Enums.ShopStatus.ACTIVE,
            CreatedAt = shop.CreatedAt
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
            Name = dto.ShopName,
            Description = dto.Description,
            OwnerAccountId = dto.OwnerId
        };
    }

    public static void MapToUpdate(this Shop shop, UpdateShopDto dto)
    {
        shop.Name = dto.ShopName;
        shop.Description = dto.Description;
        shop.UpdatedAt = DateTime.UtcNow;
    }
}
