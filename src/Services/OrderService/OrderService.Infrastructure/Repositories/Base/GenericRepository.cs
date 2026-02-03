using OrderService.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Base;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected OrderDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(OrderDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual List<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual void Create(T entity)
    {
        _context.Add(entity);
        _context.SaveChanges();
    }

    public virtual async Task<int> CreateAsync(T entity)
    {
        _context.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public virtual void Update(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
        _context.SaveChanges();
    }

    public virtual async Task<int> UpdateAsync(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
        return await _context.SaveChangesAsync();
    }

    public virtual bool Remove(T entity)
    {
        _context.Remove(entity);
        _context.SaveChanges();
        return true;
    }

    public virtual async Task<bool> RemoveAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual T? GetById(Guid id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual T? GetById(int id)
    {
        var entity = _dbSet.Find(id);
        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        return entity;
    }

    public virtual T? GetById(string code)
    {
        return _dbSet.Find(code);
    }

    public virtual async Task<T?> GetByIdAsync(string code)
    {
        return await _dbSet.FindAsync(code);
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }

    // Separated methods for unit of work pattern
    public virtual void PrepareCreate(T entity)
    {
        _context.Add(entity);
    }

    public virtual void PrepareUpdate(T entity)
    {
        var tracker = _context.Attach(entity);
        tracker.State = EntityState.Modified;
    }

    public virtual void PrepareRemove(T entity)
    {
        _context.Remove(entity);
    }

    public virtual int Save()
    {
        return _context.SaveChanges();
    }

    public virtual async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Query method
    public virtual IQueryable<T> GetAllQueryable()
    {
        return _dbSet.AsQueryable();
    }
}
