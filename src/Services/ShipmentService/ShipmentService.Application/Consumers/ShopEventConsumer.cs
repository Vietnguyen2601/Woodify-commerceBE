using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe events từ ShopService
/// Cập nhật shop info (địa chỉ, điểm giao hàng) để tính toán vận chuyển
/// </summary>
public class ShopEventConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IShopInfoCacheRepository _shopInfoCache;

    public ShopEventConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IShopInfoCacheRepository shopInfoCache)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _shopInfoCache = shopInfoCache;
    }

    public void StartListening()
    {
        // Subscribe to ShopUpdated events
        _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
            queueName: "shipmentservice.shop.updated",
            exchange: "shop.events",
            routingKey: "shop.updated",
            handler: async (message) => await HandleShopUpdated(message)
        );

        // Subscribe to ShopCreated events
        _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
            queueName: "shipmentservice.shop.created",
            exchange: "shop.events",
            routingKey: "shop.created",
            handler: async (message) => await HandleShopCreated(message)
        );

        Console.WriteLine("ShopEventConsumer started listening for Shop events");
        Console.WriteLine("Subscribed to: shop.events exchange with routing keys: shop.updated, shop.created");
    }

    private async Task HandleShopUpdated(ShopUpdatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[ShipmentService] Received ShopUpdated event: ShopId={evt.ShopId}, ShopName={evt.ShopName}");

            // Cache shop info locally (address, contact info for shipping calculations)
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
            Console.WriteLine($"[ShipmentService] Shop info cached: {shopInfo.ShopName}, Provider: {evt.DefaultProvider}, Code: {shopInfo.DefaultProviderServiceCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShipmentService] Error handling ShopUpdated: {ex.Message}");
        }
    }

    private async Task HandleShopCreated(ShopCreatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[ShipmentService] Received ShopCreated event: ShopId={evt.ShopId}, ShopName={evt.ShopName}");

            // Initialize shop info in cache
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
            Console.WriteLine($"[ShipmentService] New shop cached: {evt.ShopName}, Provider: {evt.DefaultProvider}, Code: {shopInfo.DefaultProviderServiceCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShipmentService] Error handling ShopCreated: {ex.Message}");
        }
    }
}
