using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Shared.Messaging;
using ShopService.Infrastructure.Data.Context;

namespace ShopService.Application.Consumers;

/// <summary>
/// Updates Shop.rating and Shop.review_count from ProductService aggregates (no HTTP).
/// </summary>
public class ShopReviewStatsUpdatedConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public ShopReviewStatsUpdatedConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<ShopReviewStatsUpdatedEvent>(
            queueName: "shopservice.shop.review_stats.updated",
            exchange: "shop.events",
            routingKey: "shop.review_stats.updated",
            handler: async evt =>
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                var shop = await db.Shops.AsTracking()
                    .FirstOrDefaultAsync(s => s.ShopId == evt.ShopId);
                if (shop == null)
                    return;

                shop.Rating = evt.AverageRating.HasValue
                    ? Math.Round((decimal)evt.AverageRating.Value, 2, MidpointRounding.AwayFromZero)
                    : 0m;
                shop.ReviewCount = evt.ReviewCount;
                shop.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            });

        Console.WriteLine("[ShopService] ShopReviewStatsUpdatedConsumer listening: shop.events → shop.review_stats.updated");
    }
}
