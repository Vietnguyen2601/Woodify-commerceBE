using System.Collections.Concurrent;

namespace ProductService.Application.Services;

/// <summary>
/// In-memory cache for shop names populated via RabbitMQ events.
/// Registered as Singleton so the cache persists for the lifetime of the service.
/// </summary>
public class ShopNameCacheService
{
    private readonly ConcurrentDictionary<Guid, string> _cache = new();

    public void Set(Guid shopId, string shopName)
    {
        _cache[shopId] = shopName;
    }

    public string? Get(Guid shopId)
    {
        return _cache.TryGetValue(shopId, out var name) ? name : null;
    }
}
