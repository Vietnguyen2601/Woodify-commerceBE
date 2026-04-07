using ProductService.Infrastructure.Repositories.Base;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Repositories.IRepositories;

public interface IImageUrlRepository : IGenericRepository<ImageUrl>
{
    Task<List<ImageUrl>> GetByTypeAndReferenceAsync(string imageType, Guid referenceId);
    Task<int> GetNextSortOrderAsync(string imageType, Guid referenceId);
    Task<ImageUrl?> GetPrimaryImageAsync(string imageType, Guid referenceId);
    Task<List<ImageUrl>> BulkCreateAsync(List<ImageUrl> images);
    Task<Dictionary<Guid, string?>> GetPrimaryImageBatchAsync(string imageType, IEnumerable<Guid> referenceIds);
    Task<Dictionary<Guid, List<ImageUrl>>> GetImagesBatchAsync(string imageType, IEnumerable<Guid> referenceIds);
    
    /// <summary>
    /// Get primary image with fallback chain:
    /// 1. PRODUCT image (primary)
    /// 2. PRODUCT_VERSION image (if version provided)
    /// 3. CATEGORY image (if category provided)
    /// 4. null (caller should handle with placeholder)
    /// </summary>
    Task<ImageUrl?> GetPrimaryImageWithFallbackAsync(Guid productId, Guid? productVersionId = null, Guid? categoryId = null);
}
