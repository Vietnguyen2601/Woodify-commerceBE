using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumes <see cref="ShipmentStatusChangedEvent"/> — syncs order status and triggers SignalR (via <see cref="IOrderRealtimeNotifier"/>).
/// </summary>
public class ShipmentStatusChangedConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _consumer;
    private readonly ILogger<ShipmentStatusChangedConsumer> _logger;

    public ShipmentStatusChangedConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer consumer,
        ILogger<ShipmentStatusChangedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _logger = logger;
    }

    public void StartListening()
    {
        try
        {
            _consumer.Subscribe<ShipmentStatusChangedEvent>(
                queueName: "orderservice.shipment.status.changed",
                exchange: "shipment.events",
                routingKey: "shipment.status.changed",
                handler: HandleAsync);

            _logger.LogInformation(
                "ShipmentStatusChangedConsumer listening on orderservice.shipment.status.changed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ShipmentStatusChangedConsumer");
        }
    }

    private async Task HandleAsync(ShipmentStatusChangedEvent evt)
    {
        _logger.LogInformation(
            "ShipmentStatusChanged ShipmentId={ShipmentId} OrderId={OrderId} {Prev} -> {Next}",
            evt.ShipmentId,
            evt.OrderId,
            evt.PreviousStatus,
            evt.NewStatus);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            var notifier = scope.ServiceProvider.GetService<IOrderRealtimeNotifier>();

            var result = await orderService.ApplyShipmentStatusChangedEventAsync(evt);
            if (result.Status != 200 || result.Data == null)
            {
                _logger.LogWarning(
                    "ApplyShipmentStatusChanged failed for Order {OrderId}: {Message}",
                    evt.OrderId,
                    result.Message);
                return;
            }

            if (notifier != null)
                await notifier.NotifyOrderShipmentStatusAsync(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling ShipmentStatusChanged for Order {OrderId}",
                evt.OrderId);
        }
    }
}
