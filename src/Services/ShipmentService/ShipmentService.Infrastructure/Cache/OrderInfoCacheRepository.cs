namespace ShipmentService.Infrastructure.Cache;

/// <summary>
/// In-memory cache để lưu trữ thông tin order từ RabbitMQ event
/// Thay vì phải gọi API OrderService, ShipmentService đã có dữ liệu từ event
/// </summary>
public class OrderInfoCache
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string? DeliveryAddressId { get; set; }
    public double TotalAmountCents { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IOrderInfoCacheRepository
{
    Task SaveOrderInfoAsync(OrderInfoCache info);
    Task<OrderInfoCache?> GetOrderInfoAsync(Guid orderId);
    Task RemoveOrderInfoAsync(Guid orderId);
}

/// <summary>
/// Simple in-memory implementation using Dictionary
/// For production, consider using Redis or a database
/// </summary>
public class OrderInfoCacheRepository : IOrderInfoCacheRepository
{
    private readonly Dictionary<Guid, OrderInfoCache> _cache = new();
    private readonly object _lockObj = new();

    public Task SaveOrderInfoAsync(OrderInfoCache info)
    {
        lock (_lockObj)
        {
            _cache[info.OrderId] = info;
        }
        return Task.CompletedTask;
    }

    public Task<OrderInfoCache?> GetOrderInfoAsync(Guid orderId)
    {
        lock (_lockObj)
        {
            _cache.TryGetValue(orderId, out var info);
            return Task.FromResult(info);
        }
    }

    public Task RemoveOrderInfoAsync(Guid orderId)
    {
        lock (_lockObj)
        {
            _cache.Remove(orderId);
        }
        return Task.CompletedTask;
    }
}
