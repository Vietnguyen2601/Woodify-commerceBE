using IdentityService.Infrastructure.Data.Context;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Unit of Work Implementation
/// Quản lý transaction và repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IAccountRepository? _accounts;
    private IRoleRepository? _roles;

    public UnitOfWork(AccountDbContext context)
    {
        _context = context;
    }

    public IAccountRepository Accounts => _accounts ??= new AccountRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
