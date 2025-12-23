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
        return await _dbSet.FirstOrDefaultAsync(s => s.OwnerAccountId == ownerId && s.Status == Domain.Enums.ShopStatus.ACTIVE);
    }

    public async Task<IEnumerable<Shop>> GetActiveShopsAsync()
    {
        return await _dbSet.Where(s => s.Status == Domain.Enums.ShopStatus.ACTIVE).ToListAsync();
    }
}
