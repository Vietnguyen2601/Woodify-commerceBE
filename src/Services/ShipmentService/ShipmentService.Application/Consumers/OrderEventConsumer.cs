using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Consumer lắng nghe events từ OrderService.
/// Flow:
///   OrderService → publish "order.created"
///   → Exchange "order.events" / Routing key "order.created"
///   → Queue "shipmentservice.order.created"
///   → ShipmentService xử lý: cache order info, auto create shipment
/// </summary>
public class OrderEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IOrderInfoCacheRepository _orderInfoCache;

    public OrderEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer,
        IOrderInfoCacheRepository orderInfoCache)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
        _orderInfoCache = orderInfoCache;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderCreatedEvent>(
            queueName: "shipmentservice.order.created",
            exchange: "order.events",
            routingKey: "order.created",
            handler: async (message) => await HandleOrderCreatedAsync(message)
        );

        Console.WriteLine("[ShipmentService] OrderEventConsumer started listening");
        Console.WriteLine("  Subscribed: order.events → order.created → shipmentservice.order.created");
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[ShipmentService] Received OrderCreated event: OrderId={evt.OrderId}, ShopId={evt.ShopId}");

            // Cache order info từ event - không cần gọi API nữa
            var orderInfo = new OrderInfoCache
            {
                OrderId = evt.OrderId,
                ShopId = evt.ShopId,
                AccountId = evt.AccountId,
                DeliveryAddress = evt.DeliveryAddress,
                TotalAmountCents = evt.TotalAmountCents,
                CreatedAt = evt.CreatedAt
            };

            await _orderInfoCache.SaveOrderInfoAsync(orderInfo);
            Console.WriteLine($"[ShipmentService] Order info cached: {evt.OrderId}");

            // TODO: Auto-create shipment and calculate shipping fee
            // Use cached order info instead of calling OrderService API
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ShipmentService] Error handling OrderCreated event: {ex.Message}");
        }
    }
}
