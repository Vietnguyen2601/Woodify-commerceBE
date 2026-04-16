using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

public interface IOrderDeliveredStockLedgerRepository
{
    Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(OrderDeliveredStockLedger entity, CancellationToken cancellationToken = default);
}
