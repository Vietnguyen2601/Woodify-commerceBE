using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Services;
using Shared.Events;
using Shared.Messaging;

namespace ProductService.Application.Consumers;

/// <summary>Listens to <c>order.events</c> / <c>order.product.snapshot</c> — persists order mirror for ProductService analytics.</summary>
public class OrderProductMirrorConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderProductMirrorConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderSnapshotForProductEvent>(
            queueName: "productservice.order.product.snapshot",
            exchange: "order.events",
            routingKey: "order.product.snapshot",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var ingest = scope.ServiceProvider.GetRequiredService<OrderProductMirrorIngestService>();
                await ingest.IngestAsync(evt);
            });

        Console.WriteLine("[ProductService] OrderProductMirrorConsumer listening: order.events → order.product.snapshot");
    }
}
