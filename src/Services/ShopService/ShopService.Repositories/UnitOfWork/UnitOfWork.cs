using ShopService.Repositories.DBContext;
using ShopService.Repositories.IRepositories;
using ShopService.Repositories.Repositories;

namespace ShopService.Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShopDbContext _context;
    private IShopRepository? _shopRepository;

    public UnitOfWork(ShopDbContext context)
    {
        _context = context;
    }

    public IShopRepository Shops => _shopRepository ??= new ShopRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
