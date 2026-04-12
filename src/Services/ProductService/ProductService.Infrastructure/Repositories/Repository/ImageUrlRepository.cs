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

    public async Task<int> CountByTypeAndReferenceAsync(string imageType, Guid referenceId)
    {
        return await _dbSet.CountAsync(i =>
            i.ImageType == imageType && i.ReferenceId == referenceId);
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

    public async Task<ImageUrl?> GetPrimaryImageWithFallbackAsync(Guid productId, Guid? productVersionId, Guid? categoryId)
    {
        // Step 1: Try to get PRODUCT image
        var productImage = await _dbSet
            .Where(i => i.ImageType == "PRODUCT" && i.ReferenceId == productId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (productImage != null)
            return productImage;

        // Step 2: Try to get PRODUCT_VERSION image (if version ID provided)
        if (productVersionId.HasValue && productVersionId.Value != Guid.Empty)
        {
            var versionImage = await _dbSet
                .Where(i => i.ImageType == "PRODUCT_VERSION" && i.ReferenceId == productVersionId.Value)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.CreatedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (versionImage != null)
                return versionImage;
        }

        // Step 3: Try to get CATEGORY image (if category ID provided)
        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            var categoryImage = await _dbSet
                .Where(i => i.ImageType == "CATEGORY" && i.ReferenceId == categoryId.Value)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.CreatedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (categoryImage != null)
                return categoryImage;
        }

        // Step 4: Return null or placeholder (handled by caller)
        return null;
    }

    public async Task<List<ImageUrl>> GetAllByTypeAsync(string imageType)
    {
        return await _dbSet
            .Where(i => i.ImageType == imageType.ToUpper())
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> DeleteByIdAsync(Guid imageId)
    {
        var image = await _dbSet.FindAsync(imageId);
        if (image == null) return false;

        _dbSet.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }
}
