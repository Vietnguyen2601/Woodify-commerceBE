using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IServer _server;
    private readonly CacheSettings _settings;
    private readonly IConnectionMultiplexer _redis;

    public RedisCacheService(IConnectionMultiplexer redis, CacheSettings settings)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _db = redis.GetDatabase(settings.Db);
        _server = redis.GetServer(redis.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            return default;

        try
        {
            var prefixedKey = GenerateKey(key);
            var value = await _db.StringGetAsync(prefixedKey);

            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting cache key {key}: {ex.Message}");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, int? expirySeconds = null)
    {
        if (string.IsNullOrEmpty(key) || value == null)
            return;

        try
        {
            var prefixedKey = GenerateKey(key);
            var serialized = JsonSerializer.Serialize(value);
            var expiry = expirySeconds.HasValue 
                ? TimeSpan.FromSeconds(expirySeconds.Value)
                : TimeSpan.FromSeconds(_settings.DefaultExpirySeconds);

            await _db.StringSetAsync(prefixedKey, serialized, expiry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting cache key {key}: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        try
        {
            var prefixedKey = GenerateKey(key);
            await _db.KeyDeleteAsync(prefixedKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing cache key {key}: {ex.Message}");
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return;

        try
        {
            var prefixedPattern = GenerateKey(pattern);
            var keys = await GetKeysByPatternAsync(pattern);

            if (keys.Any())
            {
                var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
                await _db.KeyDeleteAsync(redisKeys);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing cache pattern {pattern}: {ex.Message}");
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        try
        {
            var prefixedKey = GenerateKey(key);
            return await _db.KeyExistsAsync(prefixedKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking cache key existence {key}: {ex.Message}");
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return Enumerable.Empty<string>();

        try
        {
            var prefixedPattern = GenerateKey(pattern);
            var keys = _server.Keys(
                database: _settings.Db,
                pattern: prefixedPattern,
                pageSize: 10000
            );

            return await Task.FromResult(keys.Select(k => k.ToString()));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting cache keys by pattern {pattern}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public async Task FlushAllAsync()
    {
        try
        {
            await _server.FlushDatabaseAsync(_settings.Db);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error flushing cache: {ex.Message}");
        }
    }

    private string GenerateKey(string key)
    {
        return $"{_settings.Prefix}:{key}";
    }
}
