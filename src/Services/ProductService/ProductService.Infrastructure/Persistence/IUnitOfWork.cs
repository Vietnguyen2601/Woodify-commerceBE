using ProductService.Infrastructure.Repositories.IRepositories;

namespace ProductService.Infrastructure.Persistence;

/// <summary>
/// Unit of Work Interface
/// Quản lý transaction và repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductMasterRepository ProductMasters { get; }
    IProductVersionRepository ProductVersions { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
