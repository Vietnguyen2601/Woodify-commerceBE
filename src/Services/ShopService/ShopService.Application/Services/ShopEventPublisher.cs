using Shared.Events;
using Shared.Messaging;

namespace ShopService.Application.Services;

/// <summary>
/// Service để publish Shop events qua RabbitMQ
/// ShipmentService lắng nghe để có shop info: điểm giao, điểm trả
/// </summary>
public class ShopEventPublisher
{
    private readonly RabbitMQPublisher? _publisher;

    public ShopEventPublisher(RabbitMQPublisher? publisher)
    {
        _publisher = publisher;
    }

    public void PublishShopUpdated(ShopUpdatedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ShopService] WARNING: RabbitMQ publisher is not available. Skipping ShopUpdated event.");
            return;
        }

        try
        {
            _publisher.Publish("shop.events", "shop.updated", evt);
            Console.WriteLine($"[ShopService] Published ShopUpdated event: ShopId={evt.ShopId}, ShopName={evt.ShopName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShopService] Failed to publish ShopUpdated event: {ex.Message}");
        }
    }

    public void PublishShopCreated(ShopCreatedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ShopService] WARNING: RabbitMQ publisher is not available. Skipping ShopCreated event.");
            return;
        }

        try
        {
            _publisher.Publish("shop.events", "shop.created", evt);
            Console.WriteLine($"[ShopService] Published ShopCreated event: ShopId={evt.ShopId}, ShopName={evt.ShopName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShopService] Failed to publish ShopCreated event: {ex.Message}");
        }
    }
}
