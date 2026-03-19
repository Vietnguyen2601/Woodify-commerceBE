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
}
