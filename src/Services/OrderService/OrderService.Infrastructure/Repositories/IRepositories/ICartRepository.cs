using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho Cart
/// </summary>
public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetActiveCartByAccountIdAsync(Guid accountId);
    Task<Cart?> GetCartWithItemsAsync(Guid cartId);
}
