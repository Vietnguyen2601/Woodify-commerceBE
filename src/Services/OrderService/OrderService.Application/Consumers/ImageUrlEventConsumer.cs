using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Consumer để nhận ImageUrlUpdatedEvent từ ProductService
/// Sync thumbnail_url vào ProductVersionCache
/// </summary>
public class ImageUrlEventConsumer
{
    private readonly RabbitMQConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImageUrlEventConsumer> _logger;

    public ImageUrlEventConsumer(
        RabbitMQConsumer consumer,
        IServiceScopeFactory scopeFactory,
        ILogger<ImageUrlEventConsumer> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void StartListening()
    {
        try
        {
            _consumer.Subscribe<ImageUrlUpdatedEvent>(
                exchange: "product.events",
                routingKey: "image.url.updated",
                queueName: "orderservice.imageurl.updated",
                handler: async (message) => await HandleImageUrlUpdated(message)
            );

            _logger.LogInformation("ImageUrlEventConsumer started listening on queue: orderservice.imageurl.updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ImageUrlEventConsumer: {Message}", ex.Message);
            throw;
        }
    }

    private async Task HandleImageUrlUpdated(ImageUrlUpdatedEvent evt)
    {
        try
        {
            _logger.LogInformation("[OrderService] Received ImageUrlUpdatedEvent: VersionId={VersionId}, Type={EventType}", evt.VersionId, evt.EventType);

            using var scope = _scopeFactory.CreateScope();
            var cacheRepository = scope.ServiceProvider.GetRequiredService<IProductVersionCacheRepository>();

            var cache = await cacheRepository.GetByVersionIdAsync(evt.VersionId);
            if (cache != null)
            {
                cache.ThumbnailUrl = evt.ThumbnailUrl;
                cache.LastUpdated = evt.UpdatedAt;
                await cacheRepository.UpdateAsync(cache);
                _logger.LogInformation("[OrderService] Updated thumbnail for product version cache: {VersionId}", evt.VersionId);
            }
            else
            {
                _logger.LogWarning("[OrderService] Product version cache not found for VersionId={VersionId}", evt.VersionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OrderService] Error handling ImageUrlUpdatedEvent: {Message}", ex.Message);
        }
    }
}
