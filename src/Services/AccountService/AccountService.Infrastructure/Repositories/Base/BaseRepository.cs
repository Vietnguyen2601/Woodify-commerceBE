using AccountService.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories.Base;

/// <summary>
/// Generic Base Repository Implementation
/// </summary>
public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly AccountDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(AccountDbContext context)
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

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await GetByIdAsync(id) != null;
    }
}
