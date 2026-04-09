using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Application.Interfaces;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// order.created → lưu snapshot order vào cache, tính phí, publish shippingfee.calculated.
/// Shop context lấy từ cache (đổ bởi ShopEventConsumer), không gọi HTTP.
/// </summary>
public class OrderEventConsumer : BackgroundService
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly IShippingFeeCalculator _feeCalculator;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        RabbitMQPublisher rabbitMQPublisher,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache,
        IShippingFeeCalculator feeCalculator,
        ILogger<OrderEventConsumer> logger)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _rabbitMQPublisher = rabbitMQPublisher;
        _orderInfoCache = orderInfoCache;
        _shopInfoCache = shopInfoCache;
        _feeCalculator = feeCalculator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation(
                "[ShipmentService] OrderEventConsumer started (order.events → order.created)");

            _rabbitMQConsumer.Subscribe<OrderCreatedEvent>(
                queueName: "shipmentservice.order.created",
                exchange: "order.events",
                routingKey: "order.created",
                handler: async (message) => await HandleOrderCreatedAsync(message)
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("OrderEventConsumer is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OrderEventConsumer");
            throw;
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent evt)
    {
        try
        {
            _logger.LogInformation(
                "[RabbitMQ] order.created OrderId={OrderId}, ShopId={ShopId}",
                evt.OrderId, evt.ShopId);

            await _orderInfoCache.SaveOrderInfoAsync(new OrderInfoCache
            {
                OrderId = evt.OrderId,
                ShopId = evt.ShopId,
                AccountId = evt.AccountId,
                DeliveryAddress = evt.DeliveryAddress,
                TotalAmountCents = evt.TotalAmountCents,
                TotalWeightGrams = evt.TotalWeightGrams,
                ProviderServiceCode = evt.ProviderServiceCode,
                CreatedAt = evt.CreatedAt
            });

            long shippingFeeCents = await CalculateShippingFeeAsync(evt);

            _rabbitMQPublisher.Publish(
                exchange: "shipment.events",
                routingKey: "shippingfee.calculated",
                message: new ShippingFeeCalculatedEvent
                {
                    OrderId = evt.OrderId,
                    ShopId = evt.ShopId,
                    ShippingFeeCents = shippingFeeCents,
                    ProviderServiceCode = evt.ProviderServiceCode,
                    IsFreeShipping = shippingFeeCents == 0,
                    CalculatedAt = DateTime.UtcNow
                });

            _logger.LogInformation(
                "Published ShippingFeeCalculatedEvent Order {OrderId}: Fee = {Fee} cents",
                evt.OrderId, shippingFeeCents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling order.created for Order {OrderId}", evt.OrderId);
        }
    }

    private async Task<long> CalculateShippingFeeAsync(OrderCreatedEvent evt)
    {
        try
        {
            var shopInfo = await _shopInfoCache.GetShopInfoAsync(evt.ShopId);
            if (shopInfo == null)
                _logger.LogWarning("Shop {ShopId} not in cache yet; using event-only service code", evt.ShopId);

            var providerServiceCode = evt.ProviderServiceCode
                ?? shopInfo?.DefaultProviderServiceCode
                ?? ShippingServiceConstants.SERVICE_STANDARD;

            if (!ShippingServiceConstants.IsValidServiceCode(providerServiceCode))
            {
                _logger.LogWarning(
                    "Invalid service code '{Code}' for Order {OrderId}; using STANDARD",
                    providerServiceCode, evt.OrderId);
                providerServiceCode = ShippingServiceConstants.SERVICE_STANDARD;
            }

            int weightGrams = evt.TotalWeightGrams > 0 ? evt.TotalWeightGrams : 5000;
            int serviceId = ShippingServiceConstants.GetServiceId(providerServiceCode);
            var feeResult = await _feeCalculator.CalculateAsync(serviceId, weightGrams);

            return feeResult.Total;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fee calculation failed for Order {OrderId}", evt.OrderId);
            return 30000;
        }
    }
}
