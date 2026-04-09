using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Đồng bộ shop từ ShopService (shop.events) vào bảng shop_cache — không gọi HTTP.
/// </summary>
public class ShopEventConsumer : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("[ShipmentService] ShopEventConsumer listening on shop.events");

            _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
                queueName: "shipmentservice.shop.updated",
                exchange: "shop.events",
                routingKey: "shop.updated",
                handler: async (message) => await HandleShopUpdated(message)
            );

            _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
                queueName: "shipmentservice.shop.created",
                exchange: "shop.events",
                routingKey: "shop.created",
                handler: async (message) => await HandleShopCreated(message)
            );

            _rabbitMQConsumer.Subscribe<ShopDeletedEvent>(
                queueName: "shipmentservice.shop.deleted",
                exchange: "shop.events",
                routingKey: "shop.deleted",
                handler: async (message) => await HandleShopDeleted(message)
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ShopEventConsumer is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ShopEventConsumer");
            throw;
        }
    }

    private async Task HandleShopUpdated(ShopUpdatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var shopCache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();

            _logger.LogInformation(
                "[RabbitMQ] shop.updated ShopId={ShopId}, ShopName={ShopName}",
                evt.ShopId, evt.ShopName);

            var existing = await shopCache.GetShopInfoAsync(evt.ShopId);
            var ownerAccountId = evt.OwnerAccountId != Guid.Empty
                ? evt.OwnerAccountId
                : (existing?.OwnerAccountId ?? Guid.Empty);

            var shopInfo = new ShopInfoCache
            {
                ShopId = evt.ShopId,
                OwnerAccountId = ownerAccountId,
                ShopName = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode
            };

            await shopCache.SaveShopInfoAsync(shopInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopUpdated for ShopId={ShopId}", evt.ShopId);
        }
    }

    private async Task HandleShopCreated(ShopCreatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var shopCache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();

            _logger.LogInformation(
                "[RabbitMQ] shop.created ShopId={ShopId}, ShopName={ShopName}",
                evt.ShopId, evt.ShopName);

            var shopInfo = new ShopInfoCache
            {
                ShopId = evt.ShopId,
                OwnerAccountId = evt.OwnerId,
                ShopName = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode
            };

            await shopCache.SaveShopInfoAsync(shopInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopCreated for ShopId={ShopId}", evt.ShopId);
        }
    }

    private async Task HandleShopDeleted(ShopDeletedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var shopCache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();
            await shopCache.DeleteByShopIdAsync(evt.ShopId);
            _logger.LogInformation("[ShipmentService] shop.deleted → removed shop_cache row: {ShopId}", evt.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopDeleted for ShopId={ShopId}", evt.ShopId);
        }
        await Task.CompletedTask;
    }
}
