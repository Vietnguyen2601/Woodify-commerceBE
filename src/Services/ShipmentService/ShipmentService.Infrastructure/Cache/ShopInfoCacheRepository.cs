namespace ShipmentService.Infrastructure.Cache;

/// <summary>
/// In-memory cache để lưu trữ thông tin shop từ RabbitMQ event
/// Thay vì phải gọi API ShopService, ShipmentService đã có dữ liệu từ event
/// </summary>
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
    Task RemoveShopInfoAsync(Guid shopId);
}

/// <summary>
/// Simple in-memory implementation using Dictionary
/// For production, consider using Redis or a database
/// </summary>
public class ShopInfoCacheRepository : IShopInfoCacheRepository
{
    private readonly Dictionary<Guid, ShopInfoCache> _cache = new();
    private readonly object _lockObj = new();

    public Task SaveShopInfoAsync(ShopInfoCache info)
    {
        lock (_lockObj)
        {
            _cache[info.ShopId] = info;
        }
        return Task.CompletedTask;
    }

    public Task<ShopInfoCache?> GetShopInfoAsync(Guid shopId)
    {
        lock (_lockObj)
        {
            _cache.TryGetValue(shopId, out var info);
            return Task.FromResult(info);
        }
    }

    public Task RemoveShopInfoAsync(Guid shopId)
    {
        lock (_lockObj)
        {
            _cache.Remove(shopId);
        }
        return Task.CompletedTask;
    }
}
