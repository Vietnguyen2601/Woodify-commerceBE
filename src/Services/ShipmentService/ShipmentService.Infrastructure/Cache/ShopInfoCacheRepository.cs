namespace ShipmentService.Infrastructure.Cache;

public class ShopInfoCache
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public string? DefaultProviderServiceCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public interface IShopInfoCacheRepository
{
    Task SaveShopInfoAsync(ShopInfoCache info);
    Task<ShopInfoCache?> GetShopInfoAsync(Guid shopId);
}

public class ShopInfoCacheRepository : IShopInfoCacheRepository
{
    private readonly Dictionary<Guid, ShopInfoCache> _cache = new();
    private readonly object _sync = new();

    public Task SaveShopInfoAsync(ShopInfoCache info)
    {
        lock (_sync)
            _cache[info.ShopId] = info;
        return Task.CompletedTask;
    }

    public Task<ShopInfoCache?> GetShopInfoAsync(Guid shopId)
    {
        lock (_sync)
            return Task.FromResult(_cache.TryGetValue(shopId, out var s) ? s : null);
    }
}
