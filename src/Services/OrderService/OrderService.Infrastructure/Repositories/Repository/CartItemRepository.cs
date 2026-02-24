using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<CartItem?> GetByCartIdAndVersionIdAsync(Guid cartId, Guid versionId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.VersionId == versionId);
    }

    public async Task<List<CartItem>> GetItemsByCartIdAsync(Guid cartId)
    {
        return await _dbSet
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();
    }
}
