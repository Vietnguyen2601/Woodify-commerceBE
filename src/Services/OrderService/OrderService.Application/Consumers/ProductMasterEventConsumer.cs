using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Shared.Events;
using Shared.Messaging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe events từ ProductService
/// Đồng bộ ProductMaster data vào local cache
/// </summary>
public class ProductMasterEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer? _rabbitMQConsumer;

    public ProductMasterEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer? rabbitMQConsumer = null)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public void StartListening()
    {
        if (_rabbitMQConsumer == null)
        {
            Console.WriteLine("ProductMasterEventConsumer: RabbitMQConsumer is not available. Skipping listener initialization.");
            return;
        }

        // Subscribe to ProductMaster Created events
        _rabbitMQConsumer.Subscribe<ProductMasterCreatedEvent>(
            queueName: "orderservice.product.master.created",
            exchange: "product.events",
            routingKey: "product.master.created",
            handler: async (message) => await HandleProductMasterCreated(message)
        );

        // Subscribe to ProductMaster Updated events
        _rabbitMQConsumer.Subscribe<ProductMasterUpdatedEvent>(
            queueName: "orderservice.product.master.updated",
            exchange: "product.events",
            routingKey: "product.master.updated",
            handler: async (message) => await HandleProductMasterUpdated(message)
        );

        Console.WriteLine("ProductMasterEventConsumer started listening for ProductMaster events");
        Console.WriteLine("Subscribed to: product.events exchange with routing keys: product.master.created, product.master.updated");
    }

    private async Task HandleProductMasterCreated(ProductMasterCreatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductMasterCreated event: ProductId={evt.ProductId}, Name={evt.Name}, CategoryId={evt.CategoryId}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductMasterCacheRepository>();

            var cache = new ProductMasterCache
            {
                ProductId = evt.ProductId,
                ShopId = evt.ShopId,
                CategoryId = evt.CategoryId,
                Name = evt.Name,
                Description = evt.Description,
                Status = evt.Status,
                ModerationStatus = evt.ModerationStatus,
                HasVersions = evt.HasVersions,
                LastUpdated = evt.CreatedAt
            };

            await cacheRepository.CreateAsync(cache);
            Console.WriteLine($"[OrderService] ProductMaster cache created: {evt.ProductId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductMasterCreated: {ex.Message}");
        }
    }

    private async Task HandleProductMasterUpdated(ProductMasterUpdatedEvent evt)
    {
        try
        {
            Console.WriteLine($"[OrderService] Received ProductMasterUpdated event: ProductId={evt.ProductId}, Name={evt.Name}, Status={evt.Status}");

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductMasterCacheRepository>();

            var cache = new ProductMasterCache
            {
                ProductId = evt.ProductId,
                ShopId = evt.ShopId,
                CategoryId = evt.CategoryId,
                Name = evt.Name,
                Description = evt.Description,
                Status = evt.Status,
                ModerationStatus = evt.ModerationStatus,
                HasVersions = evt.HasVersions,
                LastUpdated = evt.UpdatedAt
            };

            await cacheRepository.UpsertAsync(cache);
            Console.WriteLine($"[OrderService] ProductMaster cache updated: {evt.ProductId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error handling ProductMasterUpdated: {ex.Message}");
        }
    }
}
