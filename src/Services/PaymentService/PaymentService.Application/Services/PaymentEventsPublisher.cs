using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using Shared.Events;
using Shared.Messaging;

namespace PaymentService.Application.Services;

public sealed class PaymentEventsPublisher : IPaymentEventsPublisher
{
    private readonly RabbitMQPublisher? _publisher;
    private readonly ILogger<PaymentEventsPublisher> _logger;

    public PaymentEventsPublisher(RabbitMQPublisher? publisher, ILogger<PaymentEventsPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public void PublishPaymentOrdersPaid(PaymentOrdersPaidEvent evt)
    {
        if (_publisher == null)
        {
            _logger.LogWarning(
                "RabbitMQ publisher unavailable; dropped payment.orders.paid for PaymentId {PaymentId}",
                evt.PaymentId);
            return;
        }

        try
        {
            _publisher.Publish("payment.events", "payment.orders.paid", evt);
            _logger.LogInformation(
                "Published payment.orders.paid PaymentId={PaymentId} OrderCount={Count}",
                evt.PaymentId,
                evt.OrderIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish payment.orders.paid failed for PaymentId {PaymentId}", evt.PaymentId);
        }
    }
}
