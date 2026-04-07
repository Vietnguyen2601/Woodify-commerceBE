using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Services;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// Consumer lắng nghe events từ OrderService.
/// Flow:
///   OrderService → publish "order.created"
///   → Exchange "order.events" / Routing key "order.created"
///   → Queue "shipmentservice.order.created"
///   → ShipmentService xử lý: cache order info, tính shipping fee, publish ShippingFeeCalculatedEvent
/// </summary>
public class OrderEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly IShippingFeeCalculator _feeCalculator;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer,
        RabbitMQPublisher rabbitMQPublisher,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache,
        IShippingFeeCalculator feeCalculator,
        ILogger<OrderEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
        _rabbitMQPublisher = rabbitMQPublisher;
        _orderInfoCache = orderInfoCache;
        _shopInfoCache = shopInfoCache;
        _feeCalculator = feeCalculator;
        _logger = logger;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderCreatedEvent>(
            queueName: "shipmentservice.order.created",
            exchange: "order.events",
            routingKey: "order.created",
            handler: async (message) => await HandleOrderCreatedAsync(message)
        );

        _logger.LogInformation("[ShipmentService] OrderEventConsumer started listening");
        _logger.LogInformation("  Subscribed: order.events → order.created → shipmentservice.order.created");
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent evt)
    {
        try
        {
            _logger.LogInformation(
                "Received OrderCreated event: OrderId={OrderId}, ShopId={ShopId}, Subtotal={Subtotal}",
                evt.OrderId, evt.ShopId, evt.SubtotalCents);

            // 1. Cache order info từ event
            var orderInfo = new OrderInfoCache
            {
                OrderId = evt.OrderId,
                ShopId = evt.ShopId,
                AccountId = evt.AccountId,
                DeliveryAddress = evt.DeliveryAddress,
                TotalAmountCents = evt.SubtotalCents, // Lưu subtotal tạm thời
                CreatedAt = evt.CreatedAt
            };

            await _orderInfoCache.SaveOrderInfoAsync(orderInfo);
            _logger.LogInformation("Order info cached: {OrderId}", evt.OrderId);

            // 2. Tính shipping fee
            long shippingFeeCents = await CalculateShippingFeeAsync(evt);

            // 3. Publish ShippingFeeCalculatedEvent
            var shippingFeeEvent = new ShippingFeeCalculatedEvent
            {
                OrderId = evt.OrderId,
                ShopId = evt.ShopId,
                ShippingFeeCents = shippingFeeCents,
                ProviderServiceCode = evt.ProviderServiceCode,
                IsFreeShipping = shippingFeeCents == 0,
                CalculatedAt = DateTime.UtcNow
            };

            _rabbitMQPublisher.Publish(
                exchange: "shipment.events",
                routingKey: "shippingfee.calculated",
                message: shippingFeeEvent);

            _logger.LogInformation(
                "Published ShippingFeeCalculatedEvent for Order {OrderId}: Fee = {Fee} cents",
                evt.OrderId, shippingFeeCents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCreated event for Order {OrderId}: {Message}",
                evt.OrderId, ex.Message);
        }
    }

    private async Task<long> CalculateShippingFeeAsync(OrderCreatedEvent evt)
    {
        try
        {
            // Lấy thông tin shop từ cache
            var shopInfo = await _shopInfoCache.GetShopInfoAsync(evt.ShopId);
            if (shopInfo == null)
            {
                _logger.LogWarning("Shop {ShopId} not found in cache, using default shipping fee", evt.ShopId);
                return 30000; // Default 30,000 VND
            }

            // ✨ Xác định provider service code từ ORDER EVENT hoặc shop default
            var providerServiceCode = evt.ProviderServiceCode
                ?? shopInfo.DefaultProviderServiceCode
                ?? ShippingServiceConstants.SERVICE_STANDARD;

            // Validate service code
            if (!ShippingServiceConstants.IsValidServiceCode(providerServiceCode))
            {
                _logger.LogWarning(
                    "Invalid service code '{Code}' for Order {OrderId}, defaulting to STANDARD",
                    providerServiceCode, evt.OrderId);
                providerServiceCode = ShippingServiceConstants.SERVICE_STANDARD;
            }

            // ✨ Use weight từ event (đã tính từ product cache ở OrderService)
            // Fallback tới default 5000g nếu event không có weight
            int weightGrams = evt.TotalWeightGrams > 0 ? evt.TotalWeightGrams : 5000;

            // Convert service code → service ID using standardized constants
            int serviceId = ShippingServiceConstants.GetServiceId(providerServiceCode);

            // Calculate fee using standardized formula
            var feeResult = await _feeCalculator.CalculateAsync(serviceId, weightGrams);

            _logger.LogInformation(
                "✈️ Calculated shipping fee for Order {OrderId}: Weight={Weight}g, Service={Service} (ID:{ServiceId}), Fee={Fee}đ",
                evt.OrderId, weightGrams, providerServiceCode, serviceId, feeResult.Total);

            return feeResult.Total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating shipping fee for Order {OrderId}: {Message}",
                evt.OrderId, ex.Message);
            return 30000; // Fallback to default
        }
    }
}
