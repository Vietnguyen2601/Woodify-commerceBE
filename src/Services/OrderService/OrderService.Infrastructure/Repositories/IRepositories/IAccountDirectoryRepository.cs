using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories.IRepositories;

public interface IAccountDirectoryRepository
{
    Task<Dictionary<Guid, AccountDirectoryEntry>> GetByIdsAsync(IEnumerable<Guid> accountIds);
    Task UpsertAsync(AccountDirectoryEntry entry);
    Task UpsertManyAsync(IEnumerable<AccountDirectoryEntry> entries);
}

