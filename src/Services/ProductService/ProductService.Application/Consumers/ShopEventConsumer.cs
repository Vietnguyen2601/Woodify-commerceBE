using Shared.Events;
using Shared.Messaging;
using ProductService.Application.Services;

namespace ProductService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe Shop events từ ShopService.
/// Cập nhật local in-memory cache shop name để ProductMasterService
/// sử dụng mà không cần HTTP call sang ShopService.
/// </summary>
public class ShopEventConsumer
{
    private readonly ShopNameCacheService _shopNameCache;
    private readonly RabbitMQConsumer _rabbitMQConsumer;

    public ShopEventConsumer(
        ShopNameCacheService shopNameCache,
        RabbitMQConsumer rabbitMQConsumer)
    {
        _shopNameCache = shopNameCache;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
            queueName: "productservice.shop.created",
            exchange: "shop.events",
            routingKey: "shop.created",
            handler: async (evt) =>
            {
                _shopNameCache.Set(evt.ShopId, evt.ShopName);
                Console.WriteLine($"[ProductService] Shop cache updated (created): ShopId={evt.ShopId}, ShopName={evt.ShopName}");
                await Task.CompletedTask;
            });

        _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
            queueName: "productservice.shop.updated",
            exchange: "shop.events",
            routingKey: "shop.updated",
            handler: async (evt) =>
            {
                _shopNameCache.Set(evt.ShopId, evt.ShopName);
                Console.WriteLine($"[ProductService] Shop cache updated (updated): ShopId={evt.ShopId}, ShopName={evt.ShopName}");
                await Task.CompletedTask;
            });

        Console.WriteLine("[ProductService] ShopEventConsumer started listening on shop.events exchange");
    }
}
