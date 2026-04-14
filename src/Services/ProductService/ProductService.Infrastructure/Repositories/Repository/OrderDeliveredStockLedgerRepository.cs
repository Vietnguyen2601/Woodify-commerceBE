using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories.Repository;

public class OrderDeliveredStockLedgerRepository : IOrderDeliveredStockLedgerRepository
{
    private readonly ProductDbContext _context;

    public OrderDeliveredStockLedgerRepository(ProductDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return _context.OrderDeliveredStockLedgers.AnyAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public async Task AddAsync(OrderDeliveredStockLedger entity, CancellationToken cancellationToken = default)
    {
        await _context.OrderDeliveredStockLedgers.AddAsync(entity, cancellationToken);
    }
}
