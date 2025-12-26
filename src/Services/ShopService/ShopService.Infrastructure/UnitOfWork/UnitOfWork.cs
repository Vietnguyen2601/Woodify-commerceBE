using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Repositories;
using ShopService.Infrastructure.Repositories.IRepositories;

namespace ShopService.Infrastructure.UnitOfWork;

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
