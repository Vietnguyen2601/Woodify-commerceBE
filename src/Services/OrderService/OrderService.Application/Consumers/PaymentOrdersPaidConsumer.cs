using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Application.Services;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumes <see cref="PaymentOrdersPaidEvent"/> after PayOS, wallet, or other successful payment.
/// Order aggregate status is <b>not</b> changed here (no COMPLETED from payment).
/// Publishes <see cref="OrderSellerNetEligibleEvent"/> so PaymentService credits the shop owner wallet (idempotent).
/// </summary>
public class PaymentOrdersPaidConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _consumer;
    private readonly OrderEventPublisher _orderEventPublisher;
    private readonly ILogger<PaymentOrdersPaidConsumer> _logger;

    public PaymentOrdersPaidConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer consumer,
        OrderEventPublisher orderEventPublisher,
        ILogger<PaymentOrdersPaidConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _orderEventPublisher = orderEventPublisher;
        _logger = logger;
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

                _logger.LogInformation(
                    "Payment {PaymentId} succeeded for Order {OrderId} ({Provider}); order status left unchanged ({Status})",
                    evt.PaymentId,
                    order.OrderId,
                    evt.Provider,
                    order.Status);

                var shop = await shopInfoCache.GetByShopIdAsync(order.ShopId);
                if (shop == null || shop.OwnerAccountId == Guid.Empty)
                {
                    _logger.LogWarning(
                        "Skip seller net for Order {OrderId}: shop missing or no OwnerAccountId (ShopId={ShopId})",
                        order.OrderId,
                        order.ShopId);
                    continue;
                }

                var net = (long)Math.Max(0, order.TotalAmountVnd - order.CommissionVnd);
                if (net <= 0)
                    continue;

                _orderEventPublisher.PublishOrderSellerNetEligible(new OrderSellerNetEligibleEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    SellerAccountId = shop.OwnerAccountId,
                    TotalAmountVnd = (long)order.TotalAmountVnd,
                    CommissionVnd = order.CommissionVnd,
                    NetAmountVnd = net,
                    OccurredAt = DateTime.UtcNow,
                    IdempotencyKey = $"order_net:{order.OrderId}"
                });
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
