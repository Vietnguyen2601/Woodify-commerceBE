using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.IRepositories;

namespace ProductService.Infrastructure.Repositories.Repository;

public sealed class ShopRegistryRepository : IShopRegistryRepository
{
    private readonly ProductDbContext _context;

    public ShopRegistryRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetNameAsync(Guid shopId)
    {
        return await _context.ShopRegistry
            .AsNoTracking()
            .Where(x => x.ShopId == shopId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, string>> GetNamesAsync(IEnumerable<Guid> shopIds)
    {
        var ids = shopIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        return await _context.ShopRegistry
            .AsNoTracking()
            .Where(x => ids.Contains(x.ShopId))
            .ToDictionaryAsync(x => x.ShopId, x => x.Name);
    }

    public async Task UpsertAsync(ShopRegistryEntry entry)
    {
        if (entry.ShopId == Guid.Empty)
            return;

        var existing = await _context.ShopRegistry
            .AsTracking()
            .FirstOrDefaultAsync(x => x.ShopId == entry.ShopId);

        if (existing is null)
        {
            await _context.ShopRegistry.AddAsync(new ShopRegistryEntry
            {
                ShopId = entry.ShopId,
                Name = entry.Name?.Trim() ?? string.Empty,
                UpdatedAt = entry.UpdatedAt
            });
        }
        else
        {
            existing.Name = entry.Name?.Trim() ?? string.Empty;
            existing.UpdatedAt = entry.UpdatedAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpsertManyAsync(IEnumerable<ShopRegistryEntry> entries)
    {
        var list = entries
            .Where(e => e.ShopId != Guid.Empty && !string.IsNullOrWhiteSpace(e.Name))
            .Select(e => new ShopRegistryEntry
            {
                ShopId = e.ShopId,
                Name = e.Name.Trim(),
                UpdatedAt = e.UpdatedAt
            })
            .ToList();

        if (list.Count == 0)
            return;

        var ids = list.Select(x => x.ShopId).Distinct().ToList();
        var existing = await _context.ShopRegistry
            .AsTracking()
            .Where(x => ids.Contains(x.ShopId))
            .ToDictionaryAsync(x => x.ShopId, x => x);

        foreach (var e in list)
        {
            if (existing.TryGetValue(e.ShopId, out var row))
            {
                row.Name = e.Name;
                row.UpdatedAt = e.UpdatedAt;
            }
            else
            {
                await _context.ShopRegistry.AddAsync(e);
            }
        }

        await _context.SaveChangesAsync();
    }
}

