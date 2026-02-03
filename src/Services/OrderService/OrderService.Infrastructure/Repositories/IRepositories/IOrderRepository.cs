using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Order
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(Guid orderId);
    Task<Order?> GetOrderByCodeAsync(string orderCode);
    Task<List<Order>> GetOrdersByAccountIdAsync(Guid accountId);
    Task<string> GenerateOrderCodeAsync();
}
