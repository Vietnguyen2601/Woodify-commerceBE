using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;
using ShopService.Application.Consumers;

namespace ShopService.APIService.Extensions;

/// <summary>
/// RabbitMQ Event Consumer Setup
/// Register tất cả dashboard event consumers
/// Consumers subscribe tới OrderService events
/// </summary>
public static class RabbitMQConsumerExtensions
{
    public static void SetupDashboardEventConsumers(
        this IServiceProvider serviceProvider,
        RabbitMQSettings rabbitMQSettings)
    {
        try
        {
            var consumer = new RabbitMQConsumer(rabbitMQSettings);

            // Exchange & Queue setup
            const string orderEventsExchange = "order.events";
            const string dashboardQueuePrefix = "shop.dashboard.";

            // ========================================
            // 1. OrderStatusChangedEvent Consumer
            // ========================================
            var orderStatusChangedQueueName = $"{dashboardQueuePrefix}order-status-changed";
            consumer.Subscribe<Shared.Events.OrderStatusChangedEvent>(
                orderStatusChangedQueueName,
                orderEventsExchange,
                "order.status.changed",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderStatusChangedEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 2. OrderCompletedEvent Consumer
            // ========================================
            var orderCompletedQueueName = $"{dashboardQueuePrefix}order-completed";
            consumer.Subscribe<Shared.Events.OrderCompletedEvent>(
                orderCompletedQueueName,
                orderEventsExchange,
                "order.completed",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderCompletedEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 3. OrderCancelledEvent Consumer
            // ========================================
            var orderCancelledQueueName = $"{dashboardQueuePrefix}order-cancelled";
            consumer.Subscribe<Shared.Events.OrderCancelledEvent>(
                orderCancelledQueueName,
                orderEventsExchange,
                "order.cancelled",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderCancelledEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 4. OrderRefundedEvent Consumer
            // ========================================
            var orderRefundedQueueName = $"{dashboardQueuePrefix}order-refunded";
            consumer.Subscribe<Shared.Events.OrderRefundedEvent>(
                orderRefundedQueueName,
                orderEventsExchange,
                "order.refunded",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderRefundedEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 5. OrderAwaitingPickupEvent Consumer
            // ========================================
            var orderAwaitingPickupQueueName = $"{dashboardQueuePrefix}order-awaiting-pickup";
            consumer.Subscribe<Shared.Events.OrderAwaitingPickupEvent>(
                orderAwaitingPickupQueueName,
                orderEventsExchange,
                "order.awaiting.pickup",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderAwaitingPickupEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 6. OrderReadyToShipEvent Consumer
            // ========================================
            var orderReadyToShipQueueName = $"{dashboardQueuePrefix}order-ready-to-ship";
            consumer.Subscribe<Shared.Events.OrderReadyToShipEvent>(
                orderReadyToShipQueueName,
                orderEventsExchange,
                "order.ready.to.ship",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<OrderReadyToShipEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            // ========================================
            // 7. MetricsAggregatedEvent Consumer (Batch)
            // ========================================
            var metricsAggregatedQueueName = $"{dashboardQueuePrefix}metrics-aggregated";
            consumer.Subscribe<Shared.Events.MetricsAggregatedEvent>(
                metricsAggregatedQueueName,
                orderEventsExchange,
                "metrics.aggregated",
                async (eventMessage) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<MetricsAggregatedEventConsumer>();
                    await handler.HandleAsync(eventMessage);
                });

            System.Console.WriteLine("✓ Dashboard event consumers registered successfully");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error setting up dashboard event consumers: {ex.Message}");
            // Không throw - continue without consumers
        }
    }
}

