using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Repositories.Repository;
using ProductService.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ProductService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ProductDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IProductMasterRepository? _productMasters;
    private IProductVersionRepository? _productVersions;
    private ICategoryRepository? _categories;
    private IProductReviewRepository? _productReviews;

    public UnitOfWork(ProductDbContext context)
    {
        _context = context;
    }

    public IProductMasterRepository ProductMasters => _productMasters ??= new ProductMasterRepository(_context);
    public IProductVersionRepository ProductVersions => _productVersions ??= new ProductVersionRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IProductReviewRepository ProductReviews => _productReviews ??= new ProductReviewRepository(_context);

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

    public void MarkAsModified<T>(T entity) where T : class
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
