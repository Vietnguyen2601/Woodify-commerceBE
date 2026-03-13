using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Repositories.Base;
using ShipmentService.Infrastructure.Repositories.IRepositories;

namespace ShipmentService.Infrastructure.Repositories.Repository;

public class ShippingProviderRepository : GenericRepository<ShippingProvider>, IShippingProviderRepository
{
    public ShippingProviderRepository(ShipmentDbContext context) : base(context)
    {
    }

    public override async Task<ShippingProvider?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.ProviderServices)
            .FirstOrDefaultAsync(p => p.ProviderId == id);
    }

    public async Task<List<ShippingProvider>> GetAllActiveAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Include(p => p.ProviderServices.Where(ps => ps.IsActive))
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(p => p.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludedProviderId)
    {
        return await _dbSet.AnyAsync(p =>
            p.Name.ToLower() == name.ToLower() &&
            p.ProviderId != excludedProviderId);
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(p => p.ProviderId == id);
    }
}
