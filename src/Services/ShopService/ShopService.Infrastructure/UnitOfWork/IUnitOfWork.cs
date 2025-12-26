using ShopService.Infrastructure.Repositories.IRepositories;

namespace ShopService.Infrastructure.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IShopRepository Shops { get; }
    Task<int> SaveChangesAsync();
}
