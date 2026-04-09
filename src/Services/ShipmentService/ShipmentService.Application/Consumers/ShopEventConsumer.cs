using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Đồng bộ shop từ ShopService qua RabbitMQ (shop.events) — không gọi HTTP.
/// </summary>
public class ShopEventConsumer : BackgroundService
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly ILogger<ShopEventConsumer> _logger;

    public ShopEventConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IShopInfoCacheRepository shopInfoCache,
        ILogger<ShopEventConsumer> logger)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _shopInfoCache = shopInfoCache;
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
            _logger.LogInformation(
                "[RabbitMQ] shop.updated ShopId={ShopId}, ShopName={ShopName}",
                evt.ShopId, evt.ShopName);

            var shopInfo = new ShopInfoCache
            {
                ShopId = evt.ShopId,
                ShopName = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode ?? "STD",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = evt.UpdatedAt
            };

            await _shopInfoCache.SaveShopInfoAsync(shopInfo);
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
            _logger.LogInformation(
                "[RabbitMQ] shop.created ShopId={ShopId}, ShopName={ShopName}",
                evt.ShopId, evt.ShopName);

            var shopInfo = new ShopInfoCache
            {
                ShopId = evt.ShopId,
                ShopName = evt.ShopName,
                DefaultPickupAddress = evt.DefaultPickupAddress,
                DefaultProvider = evt.DefaultProvider,
                DefaultProviderServiceCode = evt.DefaultProviderServiceCode ?? "STD",
                CreatedAt = evt.CreatedAt,
                UpdatedAt = null
            };

            await _shopInfoCache.SaveShopInfoAsync(shopInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ShopCreated for ShopId={ShopId}", evt.ShopId);
        }
    }
}
