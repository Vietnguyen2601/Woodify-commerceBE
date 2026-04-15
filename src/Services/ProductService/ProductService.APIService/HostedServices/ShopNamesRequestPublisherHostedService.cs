using Shared.Events;
using Shared.Messaging;

namespace ProductService.APIService.HostedServices;

/// <summary>
/// Sau khi consumer đã bind queue, publish một lần <see cref="ShopNamesRequestEvent"/> để ShopService
/// trả <see cref="ShopNamesPublishedEvent"/> — đồng bộ tên shop hoàn toàn qua RabbitMQ (không HTTP).
/// </summary>
public sealed class ShopNamesRequestPublisherHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ShopNamesRequestPublisherHostedService> _logger;

    public ShopNamesRequestPublisherHostedService(
        IServiceProvider services,
        ILogger<ShopNamesRequestPublisherHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = PublishWhenReadyAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task PublishWhenReadyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(500, cancellationToken);
            var publisher = _services.GetService<RabbitMQPublisher>();
            if (publisher is null)
            {
                _logger.LogWarning("Shop names request skipped: RabbitMQ publisher not available");
                return;
            }

            publisher.Publish(
                "shop.events",
                "shop.names.request",
                new ShopNamesRequestEvent
                {
                    RequestedBy = "product-service",
                    RequestedAt = DateTime.UtcNow
                });
            _logger.LogInformation("Published shop.names.request (event-driven shop name cache refill)");
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish shop.names.request");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
