using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để nhận ShippingFeeCalculatedEvent từ ShipmentService
/// Cập nhật TotalAmountVnd của Order khi shipping fee được tính xong
/// </summary>
public class ShippingFeeEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _consumer;
    private readonly ILogger<ShippingFeeEventConsumer> _logger;

    public ShippingFeeEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer consumer,
        ILogger<ShippingFeeEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _logger = logger;
    }

    public void StartListening()
    {
        try
        {
            _consumer.Subscribe<ShippingFeeCalculatedEvent>(
                queueName: "orderservice.shippingfee.calculated",
                exchange: "shipment.events",
                routingKey: "shippingfee.calculated",
                handler: async (evt) => await HandleShippingFeeCalculatedAsync(evt)
            );

            _logger.LogInformation("ShippingFeeEventConsumer started listening on queue: orderservice.shippingfee.calculated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ShippingFeeEventConsumer: {Message}", ex.Message);
        }
    }

    private async Task HandleShippingFeeCalculatedAsync(ShippingFeeCalculatedEvent evt)
    {
        _logger.LogInformation(
            "Received ShippingFeeCalculatedEvent for Order {OrderId}: Fee = {Fee} cents",
            evt.OrderId, evt.ShippingFeeVnd);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Get Order from database
            var order = await orderRepository.GetByIdAsync(evt.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", evt.OrderId);
                return;
            }

            // ✨ Shipping fee is already calculated and stored in Order.TotalAmountVnd during order creation
            // This event is for verification/auditing purpose
            long calculatedShippingFee = evt.ShippingFeeVnd;
            long storedShippingFee = (long)(order.TotalAmountVnd - order.SubtotalVnd);

            // Verify the calculation matches
            if (calculatedShippingFee == storedShippingFee)
            {
                _logger.LogInformation(
                    "✅ ShippingFee calculation verified for Order {OrderId}: {Fee} cents (OrderService & ShipmentService match)",
                    evt.OrderId, calculatedShippingFee);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ ShippingFee mismatch for Order {OrderId}: OrderService={StoredFee}đ, ShipmentService={CalculatedFee}đ",
                    evt.OrderId, storedShippingFee, calculatedShippingFee);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error verifying shipping fee for Order {OrderId}: {Message}",
                evt.OrderId, ex.Message);
        }
    }
}
