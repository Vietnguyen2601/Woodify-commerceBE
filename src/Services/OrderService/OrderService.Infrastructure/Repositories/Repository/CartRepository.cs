using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetActiveCartByAccountIdAsync(Guid accountId)
    {
        // Get the most recent cart for the account
        return await _dbSet
            .Include(c => c.CartItems)
            .Where(c => c.AccountId == accountId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<Cart?> GetCartWithItemsAsync(Guid cartId)
    {
        return await _dbSet
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CartId == cartId);
    }

    public override async Task<Cart?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CartId == id);
    }
}
