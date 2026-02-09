namespace Shared.Caching;

/// <summary>
/// Interface for cache service operations
/// Abstraction for Redis caching functionality
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a value from cache by key
    /// </summary>
    /// <typeparam name="T">Type of value to retrieve</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or null if not found</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Set a value in cache with optional expiry
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirySeconds">Expiry time in seconds (null = no expiry)</param>
    Task SetAsync<T>(string key, T value, int? expirySeconds = null);

    /// <summary>
    /// Remove a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Remove multiple values from cache by pattern
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "user:*")</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    /// <param name="key">Cache key</param>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Get all keys matching a pattern
    /// </summary>
    /// <param name="pattern">Key pattern (e.g., "user:*")</param>
    Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);

    /// <summary>
    /// Clear all cache (careful with this!)
    /// </summary>
    Task FlushAllAsync();
}
