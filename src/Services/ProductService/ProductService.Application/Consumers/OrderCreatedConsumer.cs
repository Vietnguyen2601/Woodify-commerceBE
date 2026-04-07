using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Services;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace ProductService.Application.Consumers;

/// <summary>
/// Consumer lắng nghe OrderCreatedEvent từ OrderService.
/// Cập nhật in-memory bestseller cache khi có đơn hàng mới.
/// Exchange: "order.events" / Routing key: "order.created"
/// </summary>
public class OrderCreatedConsumer
{
    private readonly BestSellerCacheService _bestSellerCache;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderCreatedConsumer(
        BestSellerCacheService bestSellerCache,
        RabbitMQConsumer rabbitMQConsumer,
        IServiceScopeFactory scopeFactory)
    {
        _bestSellerCache = bestSellerCache;
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<OrderCreatedEvent>(
            queueName: "productservice.order.created",
            exchange: "order.events",
            routingKey: "order.created",
            handler: async (evt) =>
            {
                Console.WriteLine($"[ProductService] Received order.created event: OrderId={evt.OrderId}, Items={evt.Items.Count}");
                await ProcessOrderCreatedAsync(evt);
            });

        Console.WriteLine("[ProductService] OrderCreatedConsumer started listening on order.events exchange");
    }

    private async Task ProcessOrderCreatedAsync(OrderCreatedEvent evt)
    {
        if (!evt.Items.Any()) return;

        using var scope = _scopeFactory.CreateScope();
        var versionRepo = scope.ServiceProvider.GetRequiredService<IProductVersionRepository>();

        foreach (var item in evt.Items)
        {
            var version = await versionRepo.GetByIdAsync(item.VersionId);
            if (version == null)
            {
                Console.WriteLine($"[ProductService] VersionId={item.VersionId} not found, skipping.");
                continue;
            }

            _bestSellerCache.RecordSale(version.ProductId, evt.ShopId, item.Quantity);
            Console.WriteLine($"[ProductService] Recorded sale: ProductId={version.ProductId}, ShopId={evt.ShopId}, Qty={item.Quantity}");
        }
    }
}
