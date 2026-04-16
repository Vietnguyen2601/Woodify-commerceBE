using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Events;
using Shared.Messaging;
using IdentityService.Infrastructure.Data.Context;

namespace IdentityService.Application.Consumers;

/// <summary>
/// Nhận account.names.request và publish account.names.published (bulk) để service khác mirror tên hiển thị (không HTTP).
/// </summary>
public sealed class AccountNamesRequestConsumer
{
    private readonly RabbitMQConsumer _rabbitMQConsumer;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly IServiceScopeFactory _scopeFactory;

    public AccountNamesRequestConsumer(
        RabbitMQConsumer rabbitMQConsumer,
        RabbitMQPublisher rabbitMQPublisher,
        IServiceScopeFactory scopeFactory)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
        _rabbitMQPublisher = rabbitMQPublisher;
        _scopeFactory = scopeFactory;
    }

    public void StartListening()
    {
        _rabbitMQConsumer.Subscribe<AccountNamesRequestEvent>(
            queueName: "identityservice.account.names.request",
            exchange: "identity.events",
            routingKey: "account.names.request",
            handler: async _ => await HandleAsync());

        Console.WriteLine("[IdentityService] AccountNamesRequestConsumer listening: identity.events → account.names.request");
    }

    private async Task HandleAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();

        var rows = await db.Accounts.AsNoTracking()
            .Select(a => new { a.AccountId, a.Username, a.Name, a.Email, a.IsActive })
            .ToListAsync();

        var accounts = rows.ConvertAll(r => new AccountNameRegistryEntry
        {
            AccountId = r.AccountId,
            Name = !string.IsNullOrWhiteSpace(r.Name)
                ? r.Name!.Trim()
                : (r.Username ?? string.Empty).Trim(),
            Email = r.Email ?? string.Empty,
            IsActive = r.IsActive
        });

        _rabbitMQPublisher.Publish(
            "identity.events",
            "account.names.published",
            new AccountNamesPublishedEvent
            {
                PublishedAt = DateTime.UtcNow,
                Accounts = accounts
            });

        Console.WriteLine($"[IdentityService] Published account.names.published ({accounts.Count} accounts)");
    }
}

