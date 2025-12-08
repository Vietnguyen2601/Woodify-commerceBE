using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Shared.Repositories;

/// <summary>
/// Generic Repository Implementation - Triển khai các thao tác CRUD cơ bản
/// Sử dụng EF Core DbContext
/// </summary>
public class Repository<T, TContext> : IRepository<T> 
    where T : class 
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(TContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }
}
