using ShopService.Common.DTOs;
using ShopService.Repositories.Models;

namespace ShopService.Repositories.Mapper;

public static class ShopMapper
{
    public static ShopDto ToDto(this Shop shop)
    {
        return new ShopDto
        {
            ShopId = shop.ShopId,
            ShopName = shop.ShopName,
            Description = shop.Description,
            Address = shop.Address,
            PhoneNumber = shop.PhoneNumber,
            OwnerId = shop.OwnerId,
            IsActive = shop.IsActive,
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
            ShopName = dto.ShopName,
            Description = dto.Description,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber,
            OwnerId = dto.OwnerId
        };
    }

    public static void MapToUpdate(this Shop shop, UpdateShopDto dto)
    {
        shop.ShopName = dto.ShopName;
        shop.Description = dto.Description;
        shop.Address = dto.Address;
        shop.PhoneNumber = dto.PhoneNumber;
        shop.UpdatedAt = DateTime.UtcNow;
    }
}
