namespace ShipmentService.Infrastructure.Repositories.Base;

public interface IGenericRepository<T> where T : class
{
    // CRUD
    List<T> GetAll();
    void Create(T entity);
    void Update(T entity);
    bool Remove(T entity);
    T? GetById(Guid id);
    T? GetById(int id);
    T? GetById(string code);

    // Async
    Task<List<T>> GetAllAsync();
    Task<int> CreateAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(string code);
    Task<bool> ExistsAsync(Guid id);

    // Unit-of-work helpers
    void PrepareCreate(T entity);
    void PrepareUpdate(T entity);
    void PrepareRemove(T entity);
    int Save();
    Task<int> SaveAsync();

    IQueryable<T> GetAllQueryable();
}
