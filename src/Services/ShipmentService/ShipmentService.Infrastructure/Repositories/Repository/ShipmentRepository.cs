using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Repositories.Base;
using ShipmentService.Infrastructure.Repositories.IRepositories;

namespace ShipmentService.Infrastructure.Repositories.Repository;

public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(ShipmentDbContext context) : base(context)
    {
    }

    public override async Task<Shipment?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.ProviderService)
                .ThenInclude(ps => ps!.ShippingProvider)
            .FirstOrDefaultAsync(s => s.ShipmentId == id);
    }

    public async Task<List<Shipment>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(s => s.ProviderService)
                .ThenInclude(ps => ps!.ShippingProvider)
            .Where(s => s.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<List<Shipment>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(s => s.ProviderService)
            .Where(s => s.Status == status)
            .ToListAsync();
    }

    public override async Task<List<Shipment>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.ProviderService)
                .ThenInclude(ps => ps!.ShippingProvider)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(s => s.ShipmentId == id);
    }
}
