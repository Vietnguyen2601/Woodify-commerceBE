using OrderService.Domain.Entities;
using Shared.Events;

namespace OrderService.Application.Services;

/// <summary>
/// Publishes RabbitMQ side-effects for order lifecycle (dashboard, ProductService mirror, review, stock).
/// </summary>
public sealed class OrderSideEffectPublisher
{
    private readonly OrderEventPublisher _publisher;

    public OrderSideEffectPublisher(OrderEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public void PublishOrderSnapshotForProduct(Order order)
    {
        _publisher.PublishOrderSnapshotForProduct(BuildSnapshot(order));
    }

    /// <summary>
    /// Call after order row reflects new status (HTTP update, shipment sync, or payment completion).
    /// </summary>
    public void PublishAfterStatusChange(Order order, string oldStatus, string newStatus)
    {
        PublishDashboardAndTaskBoardEvents(order, oldStatus, newStatus);
        PublishOrderSnapshotForProduct(order);

        if (newStatus.Equals("DELIVERED", StringComparison.OrdinalIgnoreCase) ||
            newStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            _publisher.PublishOrderReviewEligible(new OrderReviewEligibleEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                AccountId = order.AccountId,
                EligibleAt = DateTime.UtcNow,
                Lines = (order.OrderItems ?? Array.Empty<OrderItem>()).Select(oi => new OrderReviewEligibleLineItem
                {
                    OrderItemId = oi.OrderItemId,
                    VersionId = oi.VersionId
                }).ToList()
            });
        }

        if (newStatus.Equals("DELIVERED", StringComparison.OrdinalIgnoreCase) &&
            !oldStatus.Equals("DELIVERED", StringComparison.OrdinalIgnoreCase))
        {
            PublishOrderDeliveredStock(order);
        }
    }

    private void PublishDashboardAndTaskBoardEvents(Order order, string oldStatus, string newStatus)
    {
        try
        {
            var itemCount = order.OrderItems?.Sum(x => x.Quantity) ?? 0;
            var mainProduct = order.OrderItems?.FirstOrDefault();
            var productVersionId = mainProduct?.VersionId;

            var statusChangeEvent = new OrderStatusChangedEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                TotalAmountCents = (long)order.TotalAmountVnd,
                CommissionCents = order.CommissionVnd,
                NetAmountCents = (long)(order.TotalAmountVnd - order.CommissionVnd),
                StatusChangedAt = DateTime.UtcNow,
                OrderCreatedAt = order.CreatedAt,
                ItemCount = itemCount,
                ProductVersionId = productVersionId
            };
            _publisher.PublishOrderStatusChanged(statusChangeEvent);

            if (newStatus.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("PROCESSING", StringComparison.OrdinalIgnoreCase))
            {
                _publisher.PublishOrderAwaitingPickup(new OrderAwaitingPickupEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    AwaitingPickupAt = DateTime.UtcNow
                });
            }

            if (newStatus.Equals("READY_TO_SHIP", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("READY_TO_SHIP", StringComparison.OrdinalIgnoreCase))
            {
                _publisher.PublishOrderReadyToShip(new OrderReadyToShipEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    ReadyToShipAt = DateTime.UtcNow
                });
            }

            if (newStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                _publisher.PublishOrderCompleted(new OrderCompletedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    TotalAmountCents = (long)order.TotalAmountVnd,
                    CommissionRate = order.CommissionRate,
                    CommissionCents = order.CommissionVnd,
                    CompletedAt = DateTime.UtcNow,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                });
            }

            if (newStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                _publisher.PublishOrderCancelled(new OrderCancelledEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    CancelReason = "Order cancelled via dashboard",
                    CancelledAmountCents = (long)order.TotalAmountVnd,
                    CancelledAt = DateTime.UtcNow,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                });
            }

            if ((newStatus.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) ||
                 newStatus.Equals("REFUNDING", StringComparison.OrdinalIgnoreCase)) &&
                !oldStatus.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("REFUNDING", StringComparison.OrdinalIgnoreCase))
            {
                _publisher.PublishOrderRefunded(new OrderRefundedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    RefundAmountCents = (long)order.TotalAmountVnd,
                    RefundReason = "Order refunded via dashboard",
                    RefundedAt = DateTime.UtcNow,
                    OrderCreatedAt = order.CreatedAt,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error publishing dashboard events: {ex.Message}");
        }
    }

    private void PublishOrderDeliveredStock(Order order)
    {
        if (order.OrderItems == null || !order.OrderItems.Any())
            return;

        _publisher.PublishOrderDeliveredStock(new OrderDeliveredStockEvent
        {
            OrderId = order.OrderId,
            ShopId = order.ShopId,
            DeliveredAt = DateTime.UtcNow,
            Lines = order.OrderItems.Select(oi => new OrderDeliveredStockLineItem
            {
                VersionId = oi.VersionId,
                Quantity = oi.Quantity
            }).ToList()
        });
    }

    private static OrderSnapshotForProductEvent BuildSnapshot(Order order)
    {
        return new OrderSnapshotForProductEvent
        {
            OrderId = order.OrderId,
            ShopId = order.ShopId,
            AccountId = order.AccountId,
            Status = order.Status.ToString(),
            SubtotalVnd = order.SubtotalVnd,
            TotalAmountVnd = order.TotalAmountVnd,
            CommissionVnd = order.CommissionVnd,
            CommissionRate = order.CommissionRate,
            VoucherId = order.VoucherId,
            DeliveryAddress = order.DeliveryAddress,
            ProviderServiceCode = order.ProviderServiceCode ?? string.Empty,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            SnapshotAt = DateTime.UtcNow,
            Lines = (order.OrderItems ?? Array.Empty<OrderItem>()).Select(oi => new OrderSnapshotLineForProduct
            {
                OrderItemId = oi.OrderItemId,
                VersionId = oi.VersionId,
                Quantity = oi.Quantity,
                UnitPriceVnd = oi.UnitPriceVnd,
                LineTotalVnd = oi.LineTotalVnd
            }).ToList()
        };
    }
}
