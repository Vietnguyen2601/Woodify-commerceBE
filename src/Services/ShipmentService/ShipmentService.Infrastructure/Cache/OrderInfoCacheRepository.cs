namespace ShipmentService.Infrastructure.Cache;

public class OrderInfoCache
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string? DeliveryAddress { get; set; }
    /// <summary>Goods only (VND) — same as OrderCreatedEvent.SubtotalVnd; used for free-ship and pricing.</summary>
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    /// <summary>Cân nặng tổng (g) từ OrderCreatedEvent — không cần gọi ProductService.</summary>
    public int TotalWeightGrams { get; set; }
    public string? ProviderServiceCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IOrderInfoCacheRepository
{
    Task SaveOrderInfoAsync(OrderInfoCache info);
    Task<OrderInfoCache?> GetOrderInfoAsync(Guid orderId);
}

public class OrderInfoCacheRepository : IOrderInfoCacheRepository
{
    private readonly Dictionary<Guid, OrderInfoCache> _cache = new();
    private readonly object _sync = new();

    public Task SaveOrderInfoAsync(OrderInfoCache info)
    {
        lock (_sync)
            _cache[info.OrderId] = info;
        return Task.CompletedTask;
    }

    public Task<OrderInfoCache?> GetOrderInfoAsync(Guid orderId)
    {
        lock (_sync)
            return Task.FromResult(_cache.TryGetValue(orderId, out var o) ? o : null);
    }
}
