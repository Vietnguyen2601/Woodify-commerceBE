using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Shipping;
using Shared.Events;
using Shared.Messaging;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.Application.Consumers;

/// <summary>
/// order.created → lưu snapshot order vào cache, tính phí, publish shippingfee.calculated.
/// Shop context lấy từ cache (đổ bởi ShopEventConsumer), không gọi HTTP.
/// </summary>
public class OrderEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly ILogger<OrderEventConsumer> _logger;

    public OrderEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer,
        RabbitMQPublisher rabbitMQPublisher,
        IOrderInfoCacheRepository orderInfoCache,
        ILogger<OrderEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
        _rabbitMQPublisher = rabbitMQPublisher;
        _orderInfoCache = orderInfoCache;
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
                SubtotalVnd = evt.SubtotalVnd,
                TotalAmountVnd = evt.TotalAmountVnd,
                TotalWeightGrams = evt.TotalWeightGrams,
                ProviderServiceCode = evt.ProviderServiceCode,
                CreatedAt = evt.CreatedAt
            });

            long ShippingFeeVnd = await CalculateShippingFeeAsync(evt);

            _rabbitMQPublisher.Publish(
                exchange: "shipment.events",
                routingKey: "shippingfee.calculated",
                message: new ShippingFeeCalculatedEvent
                {
                    OrderId = evt.OrderId,
                    ShopId = evt.ShopId,
                    ShippingFeeVnd = ShippingFeeVnd,
                    ProviderServiceCode = evt.ProviderServiceCode,
                    IsFreeShipping = ShippingFeeVnd == 0,
                    CalculatedAt = DateTime.UtcNow
                });

            _logger.LogInformation(
                "Published ShippingFeeCalculatedEvent Order {OrderId}: Fee = {Fee} VND",
                evt.OrderId, ShippingFeeVnd);
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
            ShopInfoCache? shopInfo;
            using (var scope = _scopeFactory.CreateScope())
            {
                var shopRepo = scope.ServiceProvider.GetRequiredService<IShopInfoCacheRepository>();
                shopInfo = await shopRepo.GetShopInfoAsync(evt.ShopId);
            }

            if (shopInfo == null)
                _logger.LogWarning("Shop {ShopId} not in cache yet; using event-only service code", evt.ShopId);

            var providerServiceCode = evt.ProviderServiceCode
                ?? shopInfo?.DefaultProviderServiceCode
                ?? ShippingServiceConstants.DEFAULT_PROVIDER_SERVICE_CODE;

            if (!ShippingServiceConstants.IsValidServiceCode(providerServiceCode))
            {
                _logger.LogWarning(
                    "Invalid service code '{Code}' for Order {OrderId}; using {Fallback}",
                    providerServiceCode, evt.OrderId, ShippingServiceConstants.DEFAULT_PROVIDER_SERVICE_CODE);
                providerServiceCode = ShippingServiceConstants.DEFAULT_PROVIDER_SERVICE_CODE;
            }
            else
                providerServiceCode = ShippingServiceConstants.CanonicalizeProviderServiceCode(providerServiceCode);

            int weightGrams = evt.TotalWeightGrams > 0 ? evt.TotalWeightGrams : 5000;
            return ShippingPricing.FinalShippingFeeVnd(providerServiceCode, weightGrams, evt.SubtotalVnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fee calculation failed for Order {OrderId}", evt.OrderId);
            return 30000;
        }
    }
}
