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

    public OrderEventPublisher(RabbitMQPublisher? publisher = null)
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

    /// <summary>ProductService order mirror (full snapshot, line items). Exchange order.events / order.product.snapshot</summary>
    public void PublishOrderSnapshotForProduct(OrderSnapshotForProductEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderSnapshotForProduct event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.product.snapshot", evt);
            Console.WriteLine($"[OrderService] Published OrderSnapshotForProduct: OrderId={evt.OrderId}, Status={evt.Status}, Lines={evt.Lines.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderSnapshotForProduct: {ex.Message}");
        }
    }

    public void PublishOrderAwaitingPickup(OrderAwaitingPickupEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderAwaitingPickup event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.awaiting.pickup", evt);
            Console.WriteLine($"[OrderService] Published OrderAwaitingPickup: OrderId={evt.OrderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderAwaitingPickup: {ex.Message}");
        }
    }

    public void PublishOrderReadyToShip(OrderReadyToShipEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ publisher is not available. Skipping OrderReadyToShip event.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.ready.to.ship", evt);
            Console.WriteLine($"[OrderService] Published OrderReadyToShip: OrderId={evt.OrderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderReadyToShip: {ex.Message}");
        }
    }

    /// <summary>PaymentService ghi có ví seller (net) — idempotent theo OrderId.</summary>
    public void PublishOrderSellerNetEligible(OrderSellerNetEligibleEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ not available. Skipping OrderSellerNetEligible.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.seller.net.eligible", evt);
            Console.WriteLine($"[OrderService] Published OrderSellerNetEligible: OrderId={evt.OrderId}, Net={evt.NetAmountVnd}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderSellerNetEligible: {ex.Message}");
        }
    }

    /// <summary>PaymentService hoàn tác ghi có khi đơn hủy/hoàn sau COMPLETED.</summary>
    public void PublishOrderSellerNetReversed(OrderSellerNetReversedEvent evt)
    {
        if (_publisher == null)
        {
            Console.WriteLine("[OrderService] WARNING: RabbitMQ not available. Skipping OrderSellerNetReversed.");
            return;
        }

        try
        {
            _publisher.Publish("order.events", "order.seller.net.reversed", evt);
            Console.WriteLine($"[OrderService] Published OrderSellerNetReversed: OrderId={evt.OrderId}, Net={evt.NetAmountVnd}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Failed to publish OrderSellerNetReversed: {ex.Message}");
        }
    }
}


