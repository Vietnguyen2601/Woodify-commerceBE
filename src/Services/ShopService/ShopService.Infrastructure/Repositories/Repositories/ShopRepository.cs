using Microsoft.EntityFrameworkCore;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Repositories.Base;
using ShopService.Infrastructure.Repositories.IRepositories;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Repositories;

public class ShopRepository : BaseRepository<Shop>, IShopRepository
{
    public ShopRepository(ShopDbContext context) : base(context)
    {
    }

    public async Task<Shop?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.OwnerAccountId == ownerId);
    }

    public async Task<IEnumerable<Shop>> GetActiveShopsAsync()
    {
        return await _dbSet.Where(s => s.Status == Domain.Enums.ShopStatus.ACTIVE).ToListAsync();
    }

    public async Task<IEnumerable<Shop>> GetAllShopsAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        return await _dbSet.AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> ExistsWithNameExcludingAsync(string name, Guid excludeShopId)
    {
        return await _dbSet.AnyAsync(s => s.ShopId != excludeShopId && s.Name.ToLower() == name.ToLower());
    }
}
