using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShopService.Repositories.DBContext;

namespace ShopService.Repositories.Base;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ShopDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ShopDbContext context)
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
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}
