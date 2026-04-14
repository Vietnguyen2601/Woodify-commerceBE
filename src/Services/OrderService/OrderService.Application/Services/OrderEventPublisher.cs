using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Services;

/// <summary>
/// Service để publish Order events qua RabbitMQ
/// ShipmentService lắng nghe để tính toán shipping, ETA, v.v.
/// ShopService lắng nghe để cập nhật dashboard metrics
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

    /// <summary>
    /// Publish OrderStatusChangedEvent cho ShopService Dashboard
    /// </summary>
    public void PublishOrderStatusChanged(OrderStatusChangedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderStatusChanged event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.status.changed", evt);
            Console.WriteLine($"[OrderService] Published OrderStatusChanged event: OrderId={evt.OrderId}, Status={evt.NewStatus}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderStatusChanged event: {ex.Message}");
        }
    }

    /// <summary>ProductService decrements stock once per order when physically delivered (no HTTP).</summary>
    public void PublishOrderDeliveredStock(OrderDeliveredStockEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderDeliveredStock event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.delivered.stock", evt);
            Console.WriteLine($"[OrderService] Published OrderDeliveredStock: OrderId={evt.OrderId}, Lines={evt.Lines.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderDeliveredStock: {ex.Message}");
        }
    }

    /// <summary>ProductService consumes to grant review eligibility (no HTTP).</summary>
    public void PublishOrderReviewEligible(OrderReviewEligibleEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderReviewEligible event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.review_eligible", evt);
            Console.WriteLine($"[OrderService] Published OrderReviewEligible: OrderId={evt.OrderId}, Lines={evt.Lines.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderReviewEligible event: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish OrderCompletedEvent cho ShopService Dashboard
    /// </summary>
    public void PublishOrderCompleted(OrderCompletedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderCompleted event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.completed", evt);
            Console.WriteLine($"[OrderService] Published OrderCompleted event: OrderId={evt.OrderId}, Amount={evt.TotalAmountCents}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderCompleted event: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish OrderCancelledEvent cho ShopService Dashboard
    /// </summary>
    public void PublishOrderCancelled(OrderCancelledEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderCancelled event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.cancelled", evt);
            Console.WriteLine($"[OrderService] Published OrderCancelled event: OrderId={evt.OrderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderCancelled event: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish OrderRefundedEvent cho ShopService Dashboard
    /// </summary>
    public void PublishOrderRefunded(OrderRefundedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderRefunded event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.refunded", evt);
            Console.WriteLine($"[OrderService] Published OrderRefunded event: OrderId={evt.OrderId}, Amount={evt.RefundAmountCents}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderRefunded event: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish OrderCreatedForShopEvent tới ShopService khi order mới được tạo
    /// ShopService sẽ cập nhật order list, metrics và dashboard
    /// Exchange: "shop.events" / Routing key: "order.created"
    /// </summary>
    public void PublishOrderCreatedForShop(OrderCreatedForShopEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine($"[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderCreatedForShop event.");
            return;
        }

        try
        {
            _publisher.Publish("shop.events", "order.created", evt);
            Console.WriteLine($"[OrderService] Published OrderCreatedForShop event: OrderId={evt.OrderId}, ShopId={evt.ShopId}, Amount={evt.TotalAmountCents}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderCreatedForShop event: {ex.Message}");
        }
    }
}


