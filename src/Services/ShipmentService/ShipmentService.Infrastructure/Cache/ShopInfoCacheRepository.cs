namespace ShipmentService.Infrastructure.Cache;

/// <summary>DTO đọc/ghi mirror shop (map sang bảng shop_cache).</summary>
public class ShopInfoCache
{
    public Guid ShopId { get; set; }
    public Guid OwnerAccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    /// <summary>Không lưu DB (spec 5 cột); fee preview fallback STD nếu null.</summary>
    public string? DefaultProviderServiceCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public interface IShopInfoCacheRepository
{
    Task SaveShopInfoAsync(ShopInfoCache info);
    Task<ShopInfoCache?> GetShopInfoAsync(Guid shopId);
    Task DeleteByShopIdAsync(Guid shopId);
}
