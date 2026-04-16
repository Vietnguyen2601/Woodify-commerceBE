using Shared.Events;
using Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Repositories.IRepositories;

namespace ProductService.Application.Consumers;

/// <summary>
/// Consumer để lắng nghe Shop events từ ShopService.
/// Cập nhật local in-memory cache shop name để ProductMasterService
/// sử dụng mà không cần HTTP call sang ShopService.
/// </summary>
public class ShopEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;

    public ShopEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<ShopCreatedEvent>(
            queueName: "productservice.shop.created",
            exchange: "shop.events",
            routingKey: "shop.created",
            handler: async (evt) =>
            {
                await UpsertAsync(new ShopRegistryEntry
                {
                    ShopId = evt.ShopId,
                    Name = evt.ShopName,
                    UpdatedAt = evt.CreatedAt
                });
                Console.WriteLine($"[ProductService] shop.created → shop_registry: ShopId={evt.ShopId}, Name={evt.ShopName}");
            });

        _rabbitMQConsumer.Subscribe<ShopUpdatedEvent>(
            queueName: "productservice.shop.updated",
            exchange: "shop.events",
            routingKey: "shop.updated",
            handler: async (evt) =>
            {
                await UpsertAsync(new ShopRegistryEntry
                {
                    ShopId = evt.ShopId,
                    Name = evt.ShopName,
                    UpdatedAt = evt.UpdatedAt
                });
                Console.WriteLine($"[ProductService] shop.updated → shop_registry: ShopId={evt.ShopId}, Name={evt.ShopName}");
            });

        _rabbitMQConsumer.Subscribe<ShopNamesPublishedEvent>(
            queueName: "productservice.shop.names.published",
            exchange: "shop.events",
            routingKey: "shop.names.published",
            handler: async (evt) =>
            {
                var entries = evt.Shops.Select(s => new ShopRegistryEntry
                {
                    ShopId = s.ShopId,
                    Name = s.ShopName,
                    UpdatedAt = evt.PublishedAt
                });
                await UpsertManyAsync(entries);
                Console.WriteLine($"[ProductService] shop.names.published → shop_registry: {evt.Shops.Count} rows");
            });

        Console.WriteLine("[ProductService] ShopEventConsumer started listening on shop.events exchange");
    }

    private async Task UpsertAsync(ShopRegistryEntry entry)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IShopRegistryRepository>();
        await repo.UpsertAsync(entry);
    }

    private async Task UpsertManyAsync(IEnumerable<ShopRegistryEntry> entries)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IShopRegistryRepository>();
        await repo.UpsertManyAsync(entries);
    }
}
