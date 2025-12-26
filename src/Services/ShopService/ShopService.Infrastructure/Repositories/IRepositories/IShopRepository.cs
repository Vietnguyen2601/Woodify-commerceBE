using ShopService.Infrastructure.Repositories.Base;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Repositories.IRepositories;

public interface IShopRepository : IBaseRepository<Shop>
{
    Task<Shop?> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Shop>> GetActiveShopsAsync();
}
