using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Shared.Messaging;
using ShopService.Infrastructure.Data.Context;

namespace ShopService.Application.Consumers;

/// <summary>
/// Nhận <see cref="ShopNamesRequestEvent"/>, đọc toàn bộ shop từ DB, publish <see cref="ShopNamesPublishedEvent"/> (event-driven, không HTTP).
/// </summary>
public sealed class ShopNamesRequestConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly RabbitMQPublisher _rabbitPublisher;
    private readonly IServiceScopeFactory _scopeFactory;

    public ShopNamesRequestConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        RabbitMQPublisher rabbitPublisher,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _rabbitPublisher = rabbitPublisher;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<ShopNamesRequestEvent>(
            queueName: "shopservice.shop.names.request",
            exchange: "shop.events",
            routingKey: "shop.names.request",
            handler: async _ => await HandleRequestAsync());

        Console.WriteLine("[ShopService] ShopNamesRequestConsumer listening: shop.events → shop.names.request");
    }

    private async Task HandleRequestAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

        var shops = await db.Shops.AsNoTracking()
            .Select(s => new ShopNameRegistryEntry
            {
                ShopId = s.ShopId,
                ShopName = s.Name ?? string.Empty
            })
            .ToListAsync();

        var published = new ShopNamesPublishedEvent
        {
            PublishedAt = DateTime.UtcNow,
            Shops = shops
        };

        _rabbitPublisher.Publish("shop.events", "shop.names.published", published);
        Console.WriteLine($"[ShopService] Published shop.names.published ({shops.Count} shops)");
    }
}
