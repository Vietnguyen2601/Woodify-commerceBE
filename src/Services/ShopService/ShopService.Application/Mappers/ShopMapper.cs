using ShopService.Application.DTOs;
using ShopService.Domain.Entities;

namespace ShopService.Application.Mappers;

public static class ShopMapper
{
    /// <summary>
    /// Map to public DTO (ẩn thông tin ngân hàng) - dùng cho GetAllShops
    /// </summary>
    public static ShopPublicDto ToPublicDto(this Shop shop, int totalProducts, int totalOrders)
    {
        return new ShopPublicDto
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
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            Status = shop.Status.ToString(),
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt
        };
    }

    public static ShopPublicDto ToPublicDto(this Shop shop) =>
        shop.ToPublicDto(shop.TotalProducts, shop.TotalOrders);

    /// <summary>
    /// Map to public DTO list (ẩn thông tin ngân hàng)
    /// </summary>
    public static IEnumerable<ShopPublicDto> ToPublicDto(this IEnumerable<Shop> shops)
    {
        return shops.Select(s => s.ToPublicDto());
    }

    public static IEnumerable<ShopPublicDto> ToPublicDto(
        this IEnumerable<Shop> shops,
        IReadOnlyDictionary<Guid, (int TotalProducts, int TotalOrders)> totalsByShopId)
    {
        return shops.Select(s =>
        {
            totalsByShopId.TryGetValue(s.ShopId, out var t);
            return s.ToPublicDto(t.TotalProducts, t.TotalOrders);
        });
    }

    /// <summary>
    /// Map to detail DTO (hiển thị thông tin ngân hàng) - dùng cho GetShopById, GetShopByOwnerId
    /// </summary>
    public static ShopDetailDto ToDetailDto(this Shop shop, int totalProducts, int totalOrders)
    {
        return new ShopDetailDto
        {
            ShopId = shop.ShopId,
            OwnerId = shop.OwnerAccountId,
            Name = shop.Name,
            Description = shop.Description,
            LogoUrl = shop.LogoUrl,
            CoverImageUrl = shop.CoverImageUrl,
            DefaultPickupAddress = shop.DefaultPickupAddress,
            DefaultProvider = shop.DefaultProvider,
            BankName = shop.BankName,
            BankAccountNumber = shop.BankAccountNumber,
            BankAccountName = shop.BankAccountName,
            Rating = shop.Rating,
            ReviewCount = shop.ReviewCount,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            Status = shop.Status.ToString(),
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt
        };
    }

    public static ShopDetailDto ToDetailDto(this Shop shop) =>
        shop.ToDetailDto(shop.TotalProducts, shop.TotalOrders);

    /// <summary>
    /// Backward compatibility - hiển thị bank info
    /// </summary>
    public static ShopDto ToDto(this Shop shop, int totalProducts, int totalOrders)
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
            BankName = shop.BankName,
            BankAccountNumber = shop.BankAccountNumber,
            BankAccountName = shop.BankAccountName,
            Rating = shop.Rating,
            ReviewCount = shop.ReviewCount,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            Status = shop.Status.ToString(),
            CreatedAt = shop.CreatedAt,
            UpdatedAt = shop.UpdatedAt
        };
    }

    public static ShopDto ToDto(this Shop shop) =>
        shop.ToDto(shop.TotalProducts, shop.TotalOrders);

    /// <summary>
    /// Backward compatibility
    /// </summary>
    public static IEnumerable<ShopDto> ToDto(this IEnumerable<Shop> shops)
    {
        return shops.Select(s => s.ToDto());
    }

    public static IEnumerable<ShopDto> ToDto(
        this IEnumerable<Shop> shops,
        IReadOnlyDictionary<Guid, (int TotalProducts, int TotalOrders)> totalsByShopId)
    {
        return shops.Select(s =>
        {
            totalsByShopId.TryGetValue(s.ShopId, out var t);
            return s.ToDto(t.TotalProducts, t.TotalOrders);
        });
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
            DefaultProvider = dto.DefaultProvider,
            BankName = dto.BankName,
            BankAccountNumber = dto.BankAccountNumber,
            BankAccountName = dto.BankAccountName
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
        shop.BankName = dto.BankName;
        shop.BankAccountNumber = dto.BankAccountNumber;
        shop.BankAccountName = dto.BankAccountName;
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
