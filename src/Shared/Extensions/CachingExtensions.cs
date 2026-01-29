using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Shared.Extensions;

/// <summary>
/// Extension methods for registering Redis caching services
/// </summary>
public static class CachingExtensions
{

    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cacheSettings = configuration.GetSection("Redis").Get<Shared.Caching.CacheSettings>()
            ?? new Shared.Caching.CacheSettings();

        services.AddSingleton(cacheSettings);

        var options = ConfigureRedisOptions(cacheSettings);
        var redis = ConnectionMultiplexer.Connect(options);

        services.AddSingleton<IConnectionMultiplexer>(redis);

        services.AddSingleton<Shared.Caching.ICacheService>(provider =>
        {
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            var settings = provider.GetRequiredService<Shared.Caching.CacheSettings>();
            return new Shared.Caching.RedisCacheService(multiplexer, settings);
        });

        return services;
    }

    private static ConfigurationOptions ConfigureRedisOptions(Shared.Caching.CacheSettings settings)
    {
        var options = ConfigurationOptions.Parse(settings.ConnectionString);

        if (!string.IsNullOrEmpty(settings.Password))
        {
            options.Password = settings.Password;
        }

        options.Ssl = settings.EnableSsl;
        options.SyncTimeout = 5000;
        options.ConnectTimeout = 5000;
        options.AbortOnConnectFail = false;

        return options;
    }
}
