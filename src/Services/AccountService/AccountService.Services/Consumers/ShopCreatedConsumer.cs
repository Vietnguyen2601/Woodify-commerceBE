using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Messaging;

namespace AccountService.Services.Consumers;

/// <summary>
/// Background Service lắng nghe events từ RabbitMQ
/// Nhận event "shop.created" từ ShopService
/// </summary>
public class ShopCreatedConsumer : BackgroundService
{
    private readonly ILogger<ShopCreatedConsumer> _logger;
    private readonly RabbitMQConsumer _consumer;

    public ShopCreatedConsumer(
        ILogger<ShopCreatedConsumer> logger,
        RabbitMQConsumer consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎧 ShopCreatedConsumer started. Listening for 'shop.created' events...");

        // Subscribe để nhận message từ queue "shop.created"
        _consumer.Subscribe<ShopCreatedEvent>("shop.created", (shopEvent) =>
        {
            _logger.LogInformation(
                "📬 [RabbitMQ] Received shop.created event: ShopId={ShopId}, ShopName={ShopName}, OwnerId={OwnerId}",
                shopEvent.ShopId,
                shopEvent.ShopName,
                shopEvent.OwnerId
            );

            // 🔔 Xử lý business logic khi nhận được event
            ProcessShopCreatedEvent(shopEvent);
        });

        return Task.CompletedTask;
    }

    private void ProcessShopCreatedEvent(ShopCreatedEvent shopEvent)
    {
        // TODO: Implement business logic
        // Ví dụ:
        // - Gửi email chúc mừng cho owner
        // - Log vào audit database
        // - Cập nhật số lượng shop của user
        
        _logger.LogInformation(
            "✅ Processed shop.created event for Shop: {ShopName} (Owner: {OwnerId})",
            shopEvent.ShopName,
            shopEvent.OwnerId
        );
    }
}
