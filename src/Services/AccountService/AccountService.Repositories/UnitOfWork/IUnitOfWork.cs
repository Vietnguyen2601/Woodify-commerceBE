using AccountService.Repositories.IRepositories;

namespace AccountService.Repositories.UnitOfWork;

/// <summary>
/// Unit of Work Interface
/// Quản lý transaction và repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    IRoleRepository Roles { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
