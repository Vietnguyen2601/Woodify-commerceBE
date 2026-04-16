using Shared.Events;
using Shared.Messaging;

namespace IdentityService.Application.Services;

public sealed class AccountEventPublisher
{
    private readonly RabbitMQPublisher? _publisher;

    public AccountEventPublisher(RabbitMQPublisher? publisher = null)
    {
        _publisher = publisher;
    }

    public void PublishAccountCreated(AccountCreatedEvent evt)
    {
        if (_publisher == null) return;

        // Keep legacy queue for existing consumers (e.g. PaymentService wallet)
        _publisher.PublishToQueue("account.created", evt);

        // New exchange-based event for other services (e.g. OrderService)
        _publisher.Publish("identity.events", "account.created", evt);
    }

    public void PublishAccountUpdated(AccountUpdatedEvent evt)
    {
        if (_publisher == null) return;
        _publisher.Publish("identity.events", "account.updated", evt);
    }
}

