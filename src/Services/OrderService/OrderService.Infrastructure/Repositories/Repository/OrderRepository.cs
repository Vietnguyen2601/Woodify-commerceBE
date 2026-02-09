using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<Order?> GetOrderByCodeAsync(string orderCode)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
    }

    public async Task<List<Order>> GetOrdersByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.AccountId == accountId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateOrderCodeAsync()
    {
        // Generate order code format: ORD-YYYYMMDD-XXXXX
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"ORD-{today}-";
        
        // Get the last order code for today
        var lastOrder = await _dbSet
            .Where(o => o.OrderCode.StartsWith(prefix))
            .OrderByDescending(o => o.OrderCode)
            .FirstOrDefaultAsync();
        
        int sequence = 1;
        if (lastOrder != null)
        {
            // Extract sequence number from last order code
            var lastSequence = lastOrder.OrderCode.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }
        
        return $"{prefix}{sequence:D5}";
    }

    public override async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == id);
    }
}
