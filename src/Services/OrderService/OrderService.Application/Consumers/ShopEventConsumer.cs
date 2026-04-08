using Shared.Events;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để xử lý ShopCreatedEvent và ShopUpdatedEvent từ ShopService
/// Đồng bộ dữ liệu Shop vào local cache (ShopCache table)
/// </summary>
public class ShopEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly ILogger<ShopEventConsumer> _logger;

    public ShopEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer,
        ILogger<ShopEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
        _logger = logger;
    }

    public void StartListening()
    {
        // Subscribe to Shop Created events
        _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
            queueName: "orderservice.shop.created",
            exchange: "shop.events",
            routingKey: "shop.created",
            handler: async (message) => await HandleShopCreatedAsync(message)
        );

        // Subscribe to Shop Updated events
        _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
            queueName: "orderservice.shop.updated",
            exchange: "shop.events",
            routingKey: "shop.updated",
            handler: async (message) => await HandleShopUpdatedAsync(message)
        );

        _logger.LogInformation("ShopEventConsumer started listening for Shop events");
        _logger.LogInformation("Subscribed to: shop.events exchange with routing keys: shop.created, shop.updated");
    }

    /// <summary>
    /// Xử lý ShopCreatedEvent
    /// </summary>
    public async Task HandleShopCreatedAsync(ShopCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Received ShopCreatedEvent for ShopId: {ShopId}, ShopName: {ShopName}",
                @event.ShopId, @event.ShopName);

            using var scope = _scopeFactory.CreateScope();
            var shopCacheRepository = scope.ServiceProvider.GetRequiredService<IShopCacheRepository>();

            // Check if shop already exists
            var existing = await shopCacheRepository.GetByShopIdAsync(@event.ShopId);
            if (existing != null)
            {
                _logger.LogWarning("Shop already exists in cache: {ShopId}", @event.ShopId);
                return;
            }

            var shopCache = new ShopCache
            {
                ShopId = @event.ShopId,
                OwnerAccountId = @event.OwnerId,
                Name = @event.ShopName,
                DefaultPickupAddress = @event.DefaultPickupAddress,
                DefaultProvider = @event.DefaultProvider,
                DefaultProviderServiceCode = @event.DefaultProviderServiceCode,
                CreatedAt = @event.CreatedAt,
                LastSyncedAt = DateTime.UtcNow
            };

            await shopCacheRepository.CreateAsync(shopCache);
            _logger.LogInformation("ShopCache created successfully for ShopId: {ShopId}", @event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopCreatedEvent for ShopId: {ShopId}", @event.ShopId);
            throw;  // Let RabbitMQ retry
        }
    }

    /// <summary>
    /// Xử lý ShopUpdatedEvent
    /// </summary>
    public async Task HandleShopUpdatedAsync(ShopUpdatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Received ShopUpdatedEvent for ShopId: {ShopId}, ShopName: {ShopName}",
                @event.ShopId, @event.ShopName);

            using var scope = _scopeFactory.CreateScope();
            var shopCacheRepository = scope.ServiceProvider.GetRequiredService<IShopCacheRepository>();

            var shopCache = new ShopCache
            {
                ShopId = @event.ShopId,
                Name = @event.ShopName,
                ShopPhone = @event.ShopPhone,
                ShopEmail = @event.ShopEmail,
                ShopAddress = @event.ShopAddress,
                DefaultPickupAddress = @event.DefaultPickupAddress,
                DefaultProvider = @event.DefaultProvider,
                DefaultProviderServiceCode = @event.DefaultProviderServiceCode
            };

            await shopCacheRepository.UpsertAsync(shopCache);
            _logger.LogInformation("ShopCache upserted successfully for ShopId: {ShopId}", @event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopUpdatedEvent for ShopId: {ShopId}", @event.ShopId);
            throw;
        }
    }
}
