using IdentityService.Infrastructure.Repositories.IRepositories;
using IdentityService.Domain.Entities;
using Shared.Caching;

namespace IdentityService.Infrastructure.Repositories;

/// <summary>
/// Cached Account Repository - Decorator pattern
/// Wraps IAccountRepository to add Redis caching functionality
/// </summary>
public class CachedAccountRepository : IAccountRepository
{
    private readonly IAccountRepository _innerRepository;
    private readonly ICacheService _cacheService;

    // Cache key patterns
    private const string ACCOUNT_BY_ID_KEY = "account:id:{0}";
    private const string ACCOUNT_BY_USERNAME_KEY = "account:username:{0}";
    private const string ACCOUNT_BY_EMAIL_KEY = "account:email:{0}";
    private const string ALL_ACCOUNTS_KEY = "account:all";
    private const string ACCOUNT_PATTERN = "account:*";

    // Cache expiry in seconds (1 hour by default)
    private readonly int _cacheExpirySeconds = 3600;

    public CachedAccountRepository(IAccountRepository innerRepository, ICacheService cacheService)
    {
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    #region IAccountRepository Methods

    /// <summary>
    /// Get account by ID with caching
    /// </summary>
    public async Task<Account?> GetByIdAsync(Guid id)
    {
        var cacheKey = string.Format(ACCOUNT_BY_ID_KEY, id);

        // Try to get from cache
        var cachedAccount = await _cacheService.GetAsync<Account>(cacheKey);
        if (cachedAccount != null)
        {
            Console.WriteLine($"[CACHE HIT] Account retrieved from cache: {id}");
            return cachedAccount;
        }

        // Get from database
        var account = await _innerRepository.GetByIdAsync(id);
        
        // Cache the result
        if (account != null)
        {
            await _cacheService.SetAsync(cacheKey, account, _cacheExpirySeconds);
            Console.WriteLine($"[CACHE SET] Account cached: {id}");
        }

        return account;
    }

    /// <summary>
    /// Get account by username with caching
    /// </summary>
    public async Task<Account?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            return null;

        var cacheKey = string.Format(ACCOUNT_BY_USERNAME_KEY, username);

        // Try to get from cache
        var cachedAccount = await _cacheService.GetAsync<Account>(cacheKey);
        if (cachedAccount != null)
        {
            Console.WriteLine($"[CACHE HIT] Account retrieved from cache by username: {username}");
            return cachedAccount;
        }

        // Get from database
        var account = await _innerRepository.GetByUsernameAsync(username);
        
        // Cache the result
        if (account != null)
        {
            await _cacheService.SetAsync(cacheKey, account, _cacheExpirySeconds);
            Console.WriteLine($"[CACHE SET] Account cached by username: {username}");
        }

        return account;
    }

    /// <summary>
    /// Get account by email with caching
    /// </summary>
    public async Task<Account?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var cacheKey = string.Format(ACCOUNT_BY_EMAIL_KEY, email);

        // Try to get from cache
        var cachedAccount = await _cacheService.GetAsync<Account>(cacheKey);
        if (cachedAccount != null)
        {
            Console.WriteLine($"[CACHE HIT] Account retrieved from cache by email: {email}");
            return cachedAccount;
        }

        // Get from database
        var account = await _innerRepository.GetByEmailAsync(email);
        
        // Cache the result
        if (account != null)
        {
            await _cacheService.SetAsync(cacheKey, account, _cacheExpirySeconds);
            Console.WriteLine($"[CACHE SET] Account cached by email: {email}");
        }

        return account;
    }

    #endregion

    #region Delegate Methods - Invalidate Cache on Write Operations

    public List<Account> GetAll()
    {
        return _innerRepository.GetAll();
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _innerRepository.GetAllAsync();
    }

    public Account? GetById(Guid id)
    {
        return _innerRepository.GetById(id);
    }

    public Account? GetById(int id)
    {
        return _innerRepository.GetById(id);
    }

    public Account? GetById(string code)
    {
        return _innerRepository.GetById(code);
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _innerRepository.GetByIdAsync(id);
    }

    public async Task<Account?> GetByIdAsync(string code)
    {
        return await _innerRepository.GetByIdAsync(code);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _innerRepository.ExistsAsync(id);
    }

    public void Create(Account entity)
    {
        _innerRepository.Create(entity);
        InvalidateAllCacheSync();
    }

    public async Task<int> CreateAsync(Account entity)
    {
        var result = await _innerRepository.CreateAsync(entity);
        await InvalidateAllCacheAsync();
        return result;
    }

    public void Update(Account entity)
    {
        _innerRepository.Update(entity);
        InvalidateAccountCacheSync(entity.AccountId, entity.Username, entity.Email);
    }

    public async Task<int> UpdateAsync(Account entity)
    {
        var result = await _innerRepository.UpdateAsync(entity);
        await InvalidateAccountCacheAsync(entity.AccountId, entity.Username, entity.Email);
        return result;
    }

    public bool Remove(Account entity)
    {
        var result = _innerRepository.Remove(entity);
        if (result)
        {
            InvalidateAccountCacheSync(entity.AccountId, entity.Username, entity.Email);
        }
        return result;
    }

    public async Task<bool> RemoveAsync(Account entity)
    {
        var result = await _innerRepository.RemoveAsync(entity);
        if (result)
        {
            await InvalidateAccountCacheAsync(entity.AccountId, entity.Username, entity.Email);
        }
        return result;
    }

    public void PrepareCreate(Account entity)
    {
        _innerRepository.PrepareCreate(entity);
    }

    public void PrepareUpdate(Account entity)
    {
        _innerRepository.PrepareUpdate(entity);
    }

    public void PrepareRemove(Account entity)
    {
        _innerRepository.PrepareRemove(entity);
    }

    public int Save()
    {
        var result = _innerRepository.Save();
        if (result > 0)
        {
            InvalidateAllCacheSync();
        }
        return result;
    }

    public async Task<int> SaveAsync()
    {
        var result = await _innerRepository.SaveAsync();
        if (result > 0)
        {
            await InvalidateAllCacheAsync();
        }
        return result;
    }

    public IQueryable<Account> GetAllQueryable()
    {
        return _innerRepository.GetAllQueryable();
    }

    #endregion

    #region Cache Invalidation

    /// <summary>
    /// Invalidate specific account cache entries (async)
    /// </summary>
    private async Task InvalidateAccountCacheAsync(Guid accountId, string? username, string? email)
    {
        var tasks = new List<Task>();

        // Remove by ID
        tasks.Add(_cacheService.RemoveAsync(string.Format(ACCOUNT_BY_ID_KEY, accountId)));

        // Remove by username
        if (!string.IsNullOrEmpty(username))
        {
            tasks.Add(_cacheService.RemoveAsync(string.Format(ACCOUNT_BY_USERNAME_KEY, username)));
        }

        // Remove by email
        if (!string.IsNullOrEmpty(email))
        {
            tasks.Add(_cacheService.RemoveAsync(string.Format(ACCOUNT_BY_EMAIL_KEY, email)));
        }

        // Remove all accounts list
        tasks.Add(_cacheService.RemoveAsync(ALL_ACCOUNTS_KEY));

        await Task.WhenAll(tasks);
        Console.WriteLine($"[CACHE INVALIDATED] Account cache cleared for ID: {accountId}");
    }

    /// <summary>
    /// Invalidate specific account cache entries (sync)
    /// </summary>
    private void InvalidateAccountCacheSync(Guid accountId, string? username, string? email)
    {
        InvalidateAccountCacheAsync(accountId, username, email).Wait();
    }

    /// <summary>
    /// Invalidate all account cache (async)
    /// </summary>
    private async Task InvalidateAllCacheAsync()
    {
        await _cacheService.RemoveByPatternAsync(ACCOUNT_PATTERN);
        Console.WriteLine("[CACHE INVALIDATED] All account caches cleared");
    }

    /// <summary>
    /// Invalidate all account cache (sync)
    /// </summary>
    private void InvalidateAllCacheSync()
    {
        InvalidateAllCacheAsync().Wait();
    }

    #endregion
}
