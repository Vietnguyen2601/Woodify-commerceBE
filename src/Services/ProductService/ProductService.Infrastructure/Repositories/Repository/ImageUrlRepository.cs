using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.Base;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories.Repository;

public class ImageUrlRepository : GenericRepository<ImageUrl>, IImageUrlRepository
{
    public ImageUrlRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<List<ImageUrl>> GetByTypeAndReferenceAsync(string imageType, Guid referenceId)
    {
        return await _dbSet
            .Where(i => i.ImageType == imageType && i.ReferenceId == referenceId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetNextSortOrderAsync(string imageType, Guid referenceId)
    {
        var max = await _dbSet
            .Where(i => i.ImageType == imageType && i.ReferenceId == referenceId)
            .MaxAsync(i => (int?)i.SortOrder);
        return (max ?? -1) + 1;
    }

    public async Task<ImageUrl?> GetPrimaryImageAsync(string imageType, Guid referenceId)
    {
        return await _dbSet
            .Where(i => i.ImageType == imageType && i.ReferenceId == referenceId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<ImageUrl>> BulkCreateAsync(List<ImageUrl> images)
    {
        await _dbSet.AddRangeAsync(images);
        await _context.SaveChangesAsync();
        return images;
    }

    public async Task<Dictionary<Guid, string?>> GetPrimaryImageBatchAsync(string imageType, IEnumerable<Guid> referenceIds)
    {
        var ids = referenceIds.ToList();
        if (!ids.Any()) return new Dictionary<Guid, string?>();

        var images = await _dbSet
            .Where(i => i.ImageType == imageType && ids.Contains(i.ReferenceId))
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return ids.ToDictionary(
            id => id,
            id => images.FirstOrDefault(i => i.ReferenceId == id)?.OriginalUrl
        );
    }

    public async Task<Dictionary<Guid, List<ImageUrl>>> GetImagesBatchAsync(string imageType, IEnumerable<Guid> referenceIds)
    {
        var ids = referenceIds.ToList();
        if (!ids.Any()) return new Dictionary<Guid, List<ImageUrl>>();

        var images = await _dbSet
            .Where(i => i.ImageType == imageType && ids.Contains(i.ReferenceId))
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return ids.ToDictionary(
            id => id,
            id => images.Where(i => i.ReferenceId == id).ToList()
        );
    }
}
