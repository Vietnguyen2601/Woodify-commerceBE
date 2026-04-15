using Shared.Events;
using Shared.Messaging;

namespace OrderService.APIService.HostedServices;

/// <summary>
/// Publish một lần account.names.request để IdentityService trả account.names.published, refill account_directory (không HTTP).
/// </summary>
public sealed class AccountNamesRequestPublisherHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AccountNamesRequestPublisherHostedService> _logger;

    public AccountNamesRequestPublisherHostedService(
        IServiceProvider services,
        ILogger<AccountNamesRequestPublisherHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = PublishAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task PublishAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(500, cancellationToken);
            var publisher = _services.GetService<RabbitMQPublisher>();
            if (publisher is null)
            {
                _logger.LogWarning("account.names.request skipped: RabbitMQ publisher not available");
                return;
            }

            publisher.Publish(
                "identity.events",
                "account.names.request",
                new AccountNamesRequestEvent
                {
                    RequestedBy = "order-service",
                    RequestedAt = DateTime.UtcNow
                });

            _logger.LogInformation("Published account.names.request");
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish account.names.request");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

