using ShopService.Repositories.Base;
using ShopService.Repositories.Models;

namespace ShopService.Repositories.IRepositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Task<Shop?> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Shop>> GetActiveShopsAsync();
}
