using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

public interface IShopRegistryRepository
{
    Task<string?> GetNameAsync(Guid shopId);
    Task<Dictionary<Guid, string>> GetNamesAsync(IEnumerable<Guid> shopIds);
    Task UpsertAsync(ShopRegistryEntry entry);
    Task UpsertManyAsync(IEnumerable<ShopRegistryEntry> entries);
}

