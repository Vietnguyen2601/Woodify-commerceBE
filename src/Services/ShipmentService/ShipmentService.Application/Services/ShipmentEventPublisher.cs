using Shared.Events;
using Shared.Messaging;

namespace ShipmentService.Application.Services;

/// <summary>
/// Publishes shipment lifecycle events (OrderService and other consumers).
/// </summary>
public class ShipmentEventPublisher
{
    private readonly RabbitMQPublisher? _publisher;

    public ShipmentEventPublisher(RabbitMQPublisher? publisher)
    {
        _publisher = publisher;
    }

    public void PublishShipmentStatusChanged(ShipmentStatusChangedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[ShipmentService] WARNING: RabbitMQ publisher missing. Skipping ShipmentStatusChanged.");
            return;
        }

        try
        {
            _publisher.Publish("shipment.events", "shipment.status.changed", evt);
            Console.WriteLine(
                $"[ShipmentService] Published ShipmentStatusChanged: ShipmentId={evt.ShipmentId}, OrderId={evt.OrderId}, {evt.PreviousStatus} -> {evt.NewStatus}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[ShipmentService] Failed to publish ShipmentStatusChanged: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[ShipmentService] Failed to publish ShipmentStatusChanged: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException and not AccessViolationException)
        {
            Console.WriteLine($"[ShipmentService] Failed to publish ShipmentStatusChanged: {ex.Message}");
        }
    }
}
