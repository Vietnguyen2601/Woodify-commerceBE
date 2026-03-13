using Shared.Events;
using Shared.Messaging;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe events từ ShopService
/// Cập nhật shop info (địa chỉ, điểm giao hàng) để tính toán vận chuyển
/// </summary>
public class ShopEventConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;

    public ShopEventConsumer(RabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
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

            // TODO: Cache shop info locally (address, contact info for shipping calculations)
            // Store shop location, phone, email for calculating shipping routes
            var shopInfo = new
            {
                evt.ShopId,
                evt.ShopName,
                evt.ShopPhone,
                evt.ShopEmail,
                Address = $"{evt.ShopAddress}, {evt.ShopWard}, {evt.ShopDistrict}, {evt.ShopCity}, {evt.ShopProvince}",
                evt.UpdatedAt
            };

            // This would typically update a ShopInfoCache or similar repository
            Console.WriteLine($"[ShipmentService] Shop info cached: {shopInfo.ShopName} at {shopInfo.Address}");

            await Task.CompletedTask; // Placeholder for actual cache update
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

            // TODO: Initialize shop info in cache
            Console.WriteLine($"[ShipmentService] New shop registered: {evt.ShopName}");

            await Task.CompletedTask; // Placeholder for actual cache initialization
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShipmentService] Error handling ShopCreated: {ex.Message}");
        }
    }
}
