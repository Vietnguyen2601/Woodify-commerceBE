namespace Shared.Caching;

/// <summary>
/// Redis cache configuration settings
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// Redis connection string
    /// Format: host:port
    /// Example: localhost:6379
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Redis database number (0-15)
    /// </summary>
    public int Db { get; set; } = 0;

    /// <summary>
    /// Cache key prefix to avoid conflicts between services
    /// Example: "ProductService", "IdentityService"
    /// </summary>
    public string Prefix { get; set; } = "Woodify";

    /// <summary>
    /// Default expiry time in seconds for cache entries
    /// </summary>
    public int DefaultExpirySeconds { get; set; } = 3600; // 1 hour

    /// <summary>
    /// Redis password (empty if no password required)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Enable SSL connection
    /// </summary>
    public bool EnableSsl { get; set; } = false;
}
