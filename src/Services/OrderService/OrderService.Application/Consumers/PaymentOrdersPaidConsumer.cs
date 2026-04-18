using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumes <see cref="PaymentOrdersPaidEvent"/> after PayOS, wallet, or other successful payment.
/// Order aggregate status is <b>not</b> changed here (no COMPLETED from payment).
/// </summary>
public class PaymentOrdersPaidConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _consumer;
    private readonly ILogger<PaymentOrdersPaidConsumer> _logger;

    public PaymentOrdersPaidConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer consumer,
        ILogger<PaymentOrdersPaidConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
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
