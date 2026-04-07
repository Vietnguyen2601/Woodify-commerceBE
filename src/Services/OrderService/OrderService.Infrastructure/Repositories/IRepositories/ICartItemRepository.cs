using OrderService.Infrastructure.Repositories.Base;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Interface Repository cho CartItem
/// </summary>
public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<CartItem?> GetByCartIdAndVersionIdAsync(Guid cartId, Guid versionId);
    Task<List<CartItem>> GetItemsByCartIdAsync(Guid cartId);

    /// <summary>
    /// Lấy cart items từ list IDs (dùng cho selected checkout)
    /// </summary>
    Task<List<CartItem>> GetByIdsAsync(IEnumerable<Guid> cartItemIds);
}
