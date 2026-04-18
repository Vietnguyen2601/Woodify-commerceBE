using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumes PaymentOrdersPaidEvent — marks orders COMPLETED after PayOS or wallet payment.
/// </summary>
public class PaymentOrdersPaidConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _consumer;
    private readonly ILogger<PaymentOrdersPaidConsumer> _logger;
    private readonly OrderSideEffectPublisher _orderSideEffects;
    private readonly OrderEventPublisher _orderEventPublisher;

    public PaymentOrdersPaidConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer consumer,
        ILogger<PaymentOrdersPaidConsumer> logger,
        OrderSideEffectPublisher orderSideEffects,
        OrderEventPublisher orderEventPublisher)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _logger = logger;
        _orderSideEffects = orderSideEffects;
        _orderEventPublisher = orderEventPublisher;
    }

    public void StartListening()
    {
        try
        {
            _consumer.Subscribe<PaymentOrdersPaidEvent>(
                queueName: "orderservice.payment.orders.paid",
                exchange: "payment.events",
                routingKey: "payment.orders.paid",
                handler: HandleAsync);

            _logger.LogInformation(
                "PaymentOrdersPaidConsumer listening on orderservice.payment.orders.paid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start PaymentOrdersPaidConsumer");
        }
    }

    private async Task HandleAsync(PaymentOrdersPaidEvent evt)
    {
        _logger.LogInformation(
            "PaymentOrdersPaidEvent PaymentId={PaymentId} Orders={Count}",
            evt.PaymentId,
            evt.OrderIds.Count);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var shopInfoCache = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();
            var notifier = scope.ServiceProvider.GetService<IOrderRealtimeNotifier>();

            var orders = await orderRepository.GetByIdsAsync(evt.OrderIds);
            foreach (var order in orders)
            {
                if (evt.AccountId.HasValue && order.AccountId != evt.AccountId.Value)
                {
                    _logger.LogWarning(
                        "Skip Order {OrderId}: AccountId mismatch (event {EvtAccount}, order {OrderAccount})",
                        order.OrderId, evt.AccountId, order.AccountId);
                    continue;
                }

                if (order.Status != OrderStatus.PENDING)
                {
                    _logger.LogInformation(
                        "Skip Order {OrderId}: status is {Status}, expected PENDING",
                        order.OrderId, order.Status);
                    continue;
                }

                var oldStatus = order.Status.ToString();
                order.Status = OrderStatus.COMPLETED;
                order.UpdatedAt = DateTime.UtcNow;
                await orderRepository.UpdateAsync(order);

                var fullOrder = await orderRepository.GetOrderWithItemsAsync(order.OrderId);
                if (fullOrder != null)
                {
                    _orderSideEffects.PublishAfterStatusChange(fullOrder, oldStatus, OrderStatus.COMPLETED.ToString());

                    var shopRow = await shopInfoCache.GetByShopIdAsync(fullOrder.ShopId);
                    if (shopRow != null && shopRow.OwnerAccountId != Guid.Empty)
                    {
                        var net = (long)Math.Max(0, fullOrder.TotalAmountVnd - fullOrder.CommissionVnd);
                        if (net > 0)
                        {
                            _orderEventPublisher.PublishOrderSellerNetEligible(new OrderSellerNetEligibleEvent
                            {
                                OrderId = fullOrder.OrderId,
                                ShopId = fullOrder.ShopId,
                                SellerAccountId = shopRow.OwnerAccountId,
                                TotalAmountVnd = (long)fullOrder.TotalAmountVnd,
                                CommissionVnd = fullOrder.CommissionVnd,
                                NetAmountVnd = net,
                                OccurredAt = DateTime.UtcNow,
                                IdempotencyKey = $"order_net:{fullOrder.OrderId}"
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Skip seller net event: no shop owner for ShopId {ShopId}",
                            fullOrder.ShopId);
                    }
                }

                if (notifier != null)
                {
                    await notifier.NotifyOrderShipmentStatusAsync(new OrderShipmentRealtimePayload
                    {
                        ShipmentId = Guid.Empty,
                        OrderId = order.OrderId,
                        ShopId = order.ShopId,
                        AccountId = order.AccountId,
                        ShipmentPreviousStatus = string.Empty,
                        ShipmentNewStatus = string.Empty,
                        OrderPreviousStatus = OrderStatus.PENDING.ToString(),
                        OrderNewStatus = OrderStatus.COMPLETED.ToString(),
                        OrderRowUpdated = true,
                        OccurredAt = DateTime.UtcNow
                    });
                }

                _logger.LogInformation(
                    "Order {OrderId} completed after payment {PaymentId} ({Provider})",
                    order.OrderId, evt.PaymentId, evt.Provider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling PaymentOrdersPaidEvent for PaymentId {PaymentId}",
                evt.PaymentId);
        }
    }
}
