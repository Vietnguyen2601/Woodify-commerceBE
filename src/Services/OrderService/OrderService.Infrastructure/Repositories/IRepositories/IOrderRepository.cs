using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Order
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(Guid orderId);
    Task<List<Order>> GetOrdersByAccountIdAsync(Guid accountId);
    Task<List<Order>> GetOrdersByShopIdAsync(Guid shopId);

    /// <summary>
    /// Lấy nhiều orders từ list IDs (dùng cho multi-order payment)
    /// </summary>
    Task<List<Order>> GetByIdsAsync(IEnumerable<Guid> orderIds);

    /// <summary>
    /// Admin: Lấy tất cả orders có pagination và filter
    /// </summary>
    Task<(List<Order> Items, int Total)> GetAllPagedAsync(int page, int pageSize, string? status, Guid? shopId, Guid? accountId);

    /// <summary>
    /// Top N product masters by units sold trên đơn <see cref="OrderStatus.COMPLETED"/> (join version cache). Optional shop filter.
    /// </summary>
    Task<List<(Guid ProductId, long UnitsSold)>> GetTopSellingProductMasterAggregatesAsync(int productLimit, Guid? shopId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả orders có status COMPLETED (bao gồm items).
    /// Dùng cho analytics: top categories, top products.
    /// </summary>
    Task<List<Order>> GetAllCompletedOrdersAsync();
}
