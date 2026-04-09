using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Đồng bộ shop (shop.events) vào bảng shop_info_cache — cart/checkout đọc local.
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
        _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
            queueName: "orderservice.shop.updated",
            exchange: "shop.events",
            routingKey: "shop.updated",
            handler: async (evt) => await HandleShopUpdated(evt));

        _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
            queueName: "orderservice.shop.created",
            exchange: "shop.events",
            routingKey: "shop.created",
            handler: async (evt) => await HandleShopCreated(evt));

        _logger.LogInformation("ShopEventConsumer listening: shop.events (shop.created, shop.updated)");
    }

    private async Task HandleShopUpdated(ShopUpdatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();
            var existing = await cache.GetByShopIdAsync(evt.ShopId);
            await cache.UpsertAsync(new ShopInfoCache
            {
                ShopId = evt.ShopId,
                OwnerAccountId = existing?.OwnerAccountId ?? Guid.Empty,
                Name = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode,
                UpdatedAt = evt.UpdatedAt
            });
            _logger.LogInformation("[OrderService] shop.updated → shop_info_cache: {ShopId} {Name}", evt.ShopId, evt.ShopName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShopUpdated handler failed for {ShopId}", evt.ShopId);
        }
        await Task.CompletedTask;
    }

    private async Task HandleShopCreated(ShopCreatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();
            await cache.UpsertAsync(new ShopInfoCache
            {
                ShopId = evt.ShopId,
                OwnerAccountId = evt.OwnerId,
                Name = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode,
                UpdatedAt = evt.CreatedAt
            });
            _logger.LogInformation("[OrderService] shop.created → shop_info_cache: {ShopId} {Name}", evt.ShopId, evt.ShopName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShopCreated handler failed for {ShopId}", evt.ShopId);
        }
        await Task.CompletedTask;
    }
}
