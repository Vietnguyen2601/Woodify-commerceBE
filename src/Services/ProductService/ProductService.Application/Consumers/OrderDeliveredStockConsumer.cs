using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Services;
using Shared.Events;
using Shared.Messaging;

namespace ProductService.Application.Consumers;

/// <summary>Listens to <c>order.events</c> / <c>order.delivered.stock</c> and decrements stock (idempotent).</summary>
public class OrderDeliveredStockConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderDeliveredStockConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderDeliveredStockEvent>(
            queueName: "productservice.order.delivered.stock",
            exchange: "order.events",
            routingKey: "order.delivered.stock",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var ingest = scope.ServiceProvider.GetRequiredService<OrderDeliveredStockIngestService>();
                await ingest.IngestAsync(evt);
            });

        Console.WriteLine("[ProductService] OrderDeliveredStockConsumer listening: order.events → order.delivered.stock");
    }
}
