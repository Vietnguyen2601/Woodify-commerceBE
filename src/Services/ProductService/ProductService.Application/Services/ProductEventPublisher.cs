using Shared.Events;
using Shared.Messaging;

namespace ProductService.Application.Services;

/// <summary>
/// Service để publish Product events qua RabbitMQ
/// </summary>
public class ProductEventPublisher
{
    private readonly RabbitMQPublisher? _publisher;

    public ProductEventPublisher(RabbitMQPublisher? publisher)
    {
        _publisher = publisher;
    }

    public void PublishProductVersionUpdated(ProductVersionUpdatedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ProductService] WARNING: RabbitMQ publisher is not available. Skipping ProductVersionUpdated event.");
            return;
        }

        try
        {
            _publisher.Publish("product.events", "product.version.updated", evt);
            Console.WriteLine($"[ProductService] Published ProductVersionUpdated event: VersionId={evt.VersionId}, Type={evt.EventType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductService] Failed to publish ProductVersionUpdated event: {ex.Message}");
        }
    }

    public void PublishProductStatusChanged(ProductStatusChangedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ProductService] WARNING: RabbitMQ publisher is not available. Skipping ProductStatusChanged event.");
            return;
        }

        try
        {
            _publisher.Publish("product.events", "product.status.changed", evt);
            Console.WriteLine($"[ProductService] Published ProductStatusChanged event: ProductId={evt.ProductId}, Status={evt.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductService] Failed to publish ProductStatusChanged event: {ex.Message}");
        }
    }

    public void PublishProductVersionDeleted(ProductVersionDeletedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ProductService] WARNING: RabbitMQ publisher is not available. Skipping ProductVersionDeleted event.");
            return;
        }

        try
        {
            _publisher.Publish("product.events", "product.version.deleted", evt);
            Console.WriteLine($"[ProductService] Published ProductVersionDeleted event: VersionId={evt.VersionId}, ProductId={evt.ProductId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductService] Failed to publish ProductVersionDeleted event: {ex.Message}");
        }
    }

    public void PublishProductVersionRestored(ProductVersionRestoredEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[ProductService] WARNING: RabbitMQ publisher is not available. Skipping ProductVersionRestored event.");
            return;
        }

        try
        {
            _publisher.Publish("product.events", "product.version.restored", evt);
            Console.WriteLine($"[ProductService] Published ProductVersionRestored event: VersionId={evt.VersionId}, ProductId={evt.ProductId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProductService] Failed to publish ProductVersionRestored event: {ex.Message}");
        }
    }
}
