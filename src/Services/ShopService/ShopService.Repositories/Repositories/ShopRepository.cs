using Microsoft.EntityFrameworkCore;
using ShopService.Repositories.Base;
using ShopService.Repositories.DBContext;
using ShopService.Repositories.IRepositories;
using ShopService.Repositories.Models;

namespace ShopService.Repositories.Repositories;

public class ShopRepository : BaseRepository<Shop>, IShopRepository
{
    public ShopRepository(ShopDbContext context) : base(context)
    {
    }

    public async Task<Shop?> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.OwnerId == ownerId && s.IsActive);
    }

    public async Task<IEnumerable<Shop>> GetActiveShopsAsync()
    {
        return await _dbSet.Where(s => s.IsActive).ToListAsync();
    }
}
