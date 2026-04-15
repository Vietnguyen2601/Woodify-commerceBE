using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Events;
using Shared.Messaging;

namespace OrderService.Application.Consumers;

/// <summary>
/// Đồng bộ account từ IdentityService (identity.events) vào bảng account_directory — không gọi HTTP.
/// </summary>
public sealed class AccountEventConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly ILogger<AccountEventConsumer> _logger;

    public AccountEventConsumer(
        IServiceScopeFactory scopeFactory,
        RabbitMQConsumer rabbitMQConsumer,
        ILogger<AccountEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQConsumer = rabbitMQConsumer;
        _logger = logger;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<AccountCreatedEvent>(
            queueName: "orderservice.account.created",
            exchange: "identity.events",
            routingKey: "account.created",
            handler: async evt => await HandleCreatedAsync(evt));

        _rabbitMQConsumer.Subscribe<AccountUpdatedEvent>(
            queueName: "orderservice.account.updated",
            exchange: "identity.events",
            routingKey: "account.updated",
            handler: async evt => await HandleUpdatedAsync(evt));

        _rabbitMQConsumer.Subscribe<AccountNamesPublishedEvent>(
            queueName: "orderservice.account.names.published",
            exchange: "identity.events",
            routingKey: "account.names.published",
            handler: async evt => await HandleNamesPublishedAsync(evt));

        _logger.LogInformation("AccountEventConsumer listening: identity.events (created, updated, names.published)");
    }

    private async Task HandleCreatedAsync(AccountCreatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAccountDirectoryRepository>();
            var displayName = ResolveDisplayName(evt.Name, evt.Username);
            await repo.UpsertAsync(new AccountDirectoryEntry
            {
                AccountId = evt.AccountId,
                Name = displayName,
                Email = evt.Email ?? string.Empty,
                IsActive = true,
                UpdatedAt = evt.CreatedAt
            });
            _logger.LogInformation("[OrderService] account.created → account_directory: {AccountId} {Name}", evt.AccountId, displayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountCreated handler failed for {AccountId}", evt.AccountId);
        }
    }

    private async Task HandleUpdatedAsync(AccountUpdatedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAccountDirectoryRepository>();
            var displayName = ResolveDisplayName(evt.Name, evt.Username);
            await repo.UpsertAsync(new AccountDirectoryEntry
            {
                AccountId = evt.AccountId,
                Name = displayName,
                Email = evt.Email ?? string.Empty,
                IsActive = evt.IsActive,
                UpdatedAt = evt.UpdatedAt
            });
            _logger.LogInformation("[OrderService] account.updated → account_directory: {AccountId} {Name}", evt.AccountId, displayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountUpdated handler failed for {AccountId}", evt.AccountId);
        }
    }

    private async Task HandleNamesPublishedAsync(AccountNamesPublishedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IAccountDirectoryRepository>();
            var list = evt.Accounts.Select(a => new AccountDirectoryEntry
            {
                AccountId = a.AccountId,
                Name = a.Name,
                Email = a.Email ?? string.Empty,
                IsActive = a.IsActive,
                UpdatedAt = evt.PublishedAt
            });
            await repo.UpsertManyAsync(list);
            _logger.LogInformation("[OrderService] account.names.published → account_directory: {Count} rows", evt.Accounts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccountNamesPublished handler failed");
        }
    }

    private static string ResolveDisplayName(string? name, string username)
    {
        if (!string.IsNullOrWhiteSpace(name))
            return name.Trim();
        return (username ?? string.Empty).Trim();
    }
}

