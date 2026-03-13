using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Services;

/// <summary>
/// Service để publish Order events qua RabbitMQ
/// ShipmentService lắng nghe để tính toán shipping, ETA, v.v.
/// </summary>
public class OrderEventPublisher
{
    private readonly RabbitMQPublisher? _publisher;

    public OrderEventPublisher(RabbitMQPublisher? publisher)
    {
        _publisher = publisher;
    }

    public void PublishOrderCreated(OrderCreatedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderCreated event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.created", evt);
            Console.WriteLine($"[OrderService] Published OrderCreated event: OrderId={evt.OrderId}, ShopId={evt.ShopId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderCreated event: {ex.Message}");
        }
    }
}
