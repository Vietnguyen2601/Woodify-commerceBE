using System.Collections.Concurrent;

namespace ProductService.Application.Services;

/// <summary>
/// In-memory cache cho bestseller data, populated bởi OrderCreatedConsumer qua RabbitMQ.
/// Registered as Singleton — data tồn tại suốt lifetime của service.
/// Key: ProductMasterId → (TotalSold, ShopId)
/// </summary>
public class BestSellerCacheService
{
    private readonly ConcurrentDictionary<Guid, int> _salesCount = new();
    private readonly ConcurrentDictionary<Guid, Guid> _productShop = new();

    public void RecordSale(Guid productMasterId, Guid shopId, int quantity)
    {
        _salesCount.AddOrUpdate(productMasterId, quantity, (_, old) => old + quantity);
        _productShop.TryAdd(productMasterId, shopId);
    }

    public int GetSoldCount(Guid productMasterId)
    {
        return _salesCount.TryGetValue(productMasterId, out var count) ? count : 0;
    }

    /// <summary>Trả về top N sản phẩm bán chạy toàn sàn (ProductMasterId, SoldCount)</summary>
    public IReadOnlyList<(Guid ProductId, int SoldCount)> GetTopSelling(int topN)
    {
        return _salesCount
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    /// <summary>Trả về top N sản phẩm bán chạy của một shop cụ thể</summary>
    public IReadOnlyList<(Guid ProductId, int SoldCount)> GetTopSellingByShop(Guid shopId, int topN)
    {
        return _salesCount
            .Where(kv => _productShop.TryGetValue(kv.Key, out var sid) && sid == shopId)
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }
}
