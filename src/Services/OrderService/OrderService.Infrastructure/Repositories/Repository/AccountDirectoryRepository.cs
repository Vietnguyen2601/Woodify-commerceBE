using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.IRepositories;

namespace OrderService.Infrastructure.Repositories.Repository;

public sealed class AccountDirectoryRepository : IAccountDirectoryRepository
{
    private readonly OrderDbContext _db;

    public AccountDirectoryRepository(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<Guid, AccountDirectoryEntry>> GetByIdsAsync(IEnumerable<Guid> accountIds)
    {
        var ids = accountIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<Guid, AccountDirectoryEntry>();

        return await _db.AccountDirectory
            .AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, x => x);
    }

    public async Task UpsertAsync(AccountDirectoryEntry entry)
    {
        if (entry.AccountId == Guid.Empty)
            return;

        var row = await _db.AccountDirectory.AsTracking()
            .FirstOrDefaultAsync(x => x.AccountId == entry.AccountId);

        if (row is null)
        {
            await _db.AccountDirectory.AddAsync(new AccountDirectoryEntry
            {
                AccountId = entry.AccountId,
                Name = entry.Name?.Trim() ?? string.Empty,
                Email = (entry.Email ?? string.Empty).Trim(),
                IsActive = entry.IsActive,
                UpdatedAt = entry.UpdatedAt
            });
        }
        else
        {
            row.Name = entry.Name?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(entry.Email))
                row.Email = entry.Email.Trim();
            row.IsActive = entry.IsActive;
            row.UpdatedAt = entry.UpdatedAt;
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpsertManyAsync(IEnumerable<AccountDirectoryEntry> entries)
    {
        var list = entries
            .Where(e => e.AccountId != Guid.Empty && !string.IsNullOrWhiteSpace(e.Name))
            .Select(e => new AccountDirectoryEntry
            {
                AccountId = e.AccountId,
                Name = e.Name.Trim(),
                Email = (e.Email ?? string.Empty).Trim(),
                IsActive = e.IsActive,
                UpdatedAt = e.UpdatedAt
            })
            .ToList();

        if (list.Count == 0)
            return;

        var ids = list.Select(x => x.AccountId).Distinct().ToList();
        var existing = await _db.AccountDirectory.AsTracking()
            .Where(x => ids.Contains(x.AccountId))
            .ToDictionaryAsync(x => x.AccountId, x => x);

        foreach (var e in list)
        {
            if (existing.TryGetValue(e.AccountId, out var row))
            {
                row.Name = e.Name;
                if (!string.IsNullOrWhiteSpace(e.Email))
                    row.Email = e.Email.Trim();
                row.IsActive = e.IsActive;
                row.UpdatedAt = e.UpdatedAt;
            }
            else
            {
                await _db.AccountDirectory.AddAsync(e);
            }
        }

        await _db.SaveChangesAsync();
    }
}

