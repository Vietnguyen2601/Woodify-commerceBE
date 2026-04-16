using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Services;
using Shared.Events;
using Shared.Messaging;

namespace ProductService.Application.Consumers;

/// <summary>Listens to <c>order.events</c> / <c>order.review_eligible</c> and upserts purchase eligibility rows.</summary>
public class OrderReviewEligibilityConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderReviewEligibilityConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderReviewEligibleEvent>(
            queueName: "productservice.order.review_eligible",
            exchange: "order.events",
            routingKey: "order.review_eligible",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var ingest = scope.ServiceProvider.GetRequiredService<OrderReviewEligibilityIngestService>();
                await ingest.IngestAsync(evt);
            });

        Console.WriteLine("[ProductService] OrderReviewEligibilityConsumer listening: order.events → order.review_eligible");
    }
}
