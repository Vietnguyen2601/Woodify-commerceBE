using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Repositories.Base;
using ShipmentService.Infrastructure.Repositories.IRepositories;

namespace ShipmentService.Infrastructure.Repositories.Repository;

public class ProviderServiceRepository : GenericRepository<ProviderService>, IProviderServiceRepository
{
    public ProviderServiceRepository(ShipmentDbContext context) : base(context)
    {
    }

    public override async Task<ProviderService?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .FirstOrDefaultAsync(ps => ps.ServiceId == id);
    }

    public async Task<List<ProviderService>> GetByProviderIdAsync(Guid providerId)
    {
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .Where(ps => ps.ProviderId == providerId)
            .ToListAsync();
    }

    public async Task<List<ProviderService>> GetAllActiveAsync()
    {
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .Where(ps => ps.IsActive)
            .ToListAsync();
    }

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
    public async Task<ProviderService?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .FirstOrDefaultAsync(ps => ps.Code == code && ps.IsActive);
    }

    public async Task<ProviderService?> GetByShopIdAndCodeAsync(Guid shopId, string code)
    {
        // shopId reserved for future shop-to-provider mapping; currently looks up by code only
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .FirstOrDefaultAsync(ps => ps.Code == code && ps.IsActive);
<<<<<<< HEAD
=======
    public async Task<bool> HasActiveByProviderIdAsync(Guid providerId)
    {
        return await _dbSet.AnyAsync(ps =>
            ps.ProviderId == providerId &&
            ps.IsActive);
    }

    public async Task<bool> ExistsByCodeForProviderAsync(Guid providerId, string code)
    {
        return await _dbSet.AnyAsync(ps =>
            ps.ProviderId == providerId &&
            ps.Code.ToLower() == code.ToLower());
>>>>>>> develop
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
    }

    public override async Task<List<ProviderService>> GetAllAsync()
    {
        return await _dbSet
            .Include(ps => ps.ShippingProvider)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(ps => ps.ServiceId == id);
    }
}
