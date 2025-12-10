using ShopService.Repositories.IRepositories;

namespace ShopService.Repositories.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IShopRepository Shops { get; }
    Task<int> SaveChangesAsync();
}
