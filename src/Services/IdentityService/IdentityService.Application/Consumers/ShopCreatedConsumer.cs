using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Messaging;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Consumers;

/// <summary>
/// Background Service lắng nghe events từ RabbitMQ
/// Nhận event "shop.created" từ ShopService
/// Tự động đổi role Customer → Seller khi user tạo shop
/// </summary>
public class ShopCreatedConsumer : BackgroundService
{
    private readonly ILogger<ShopCreatedConsumer> _logger;
    private readonly RabbitMQConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public ShopCreatedConsumer(
        ILogger<ShopCreatedConsumer> logger,
        RabbitMQConsumer consumer,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ShopCreatedConsumer started. Listening for 'shop.created' events...");

        // Subscribe để nhận message từ queue "shop.created"
        _consumer.Subscribe<ShopCreatedEvent>("shop.created", (shopEvent) =>
        {
            _logger.LogInformation(
                "[RabbitMQ] Received shop.created event: ShopId={ShopId}, ShopName={ShopName}, OwnerId={OwnerId}",
                shopEvent.ShopId,
                shopEvent.ShopName,
                shopEvent.OwnerId
            );

            ProcessShopCreatedEventAsync(shopEvent).GetAwaiter().GetResult();
        });

        return Task.CompletedTask;
    }

    private async Task ProcessShopCreatedEventAsync(ShopCreatedEvent shopEvent)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Tìm role Seller
            var sellerRole = await unitOfWork.Roles.GetByNameAsync("Seller");
            if (sellerRole == null)
            {
                _logger.LogError("Role 'Seller' not found in database. Cannot update role for OwnerId={OwnerId}", shopEvent.OwnerId);
                return;
            }

            var account = await unitOfWork.Accounts.GetByIdAsync(shopEvent.OwnerId);
            if (account == null)
            {
                _logger.LogError("Account not found for OwnerId={OwnerId}. Cannot update role.", shopEvent.OwnerId);
                return;
            }

            // Kiểm tra nếu đã là Seller thì không cần đổi
            if (account.RoleId == sellerRole.RoleId)
            {
                _logger.LogInformation("Account {OwnerId} already has Seller role. Skipping.", shopEvent.OwnerId);
                return;
            }

            // Cập nhật role từ Customer → Seller
            var oldRoleId = account.RoleId;
            account.RoleId = sellerRole.RoleId;
            account.Role = null; // Ngắt object Role cũ để tránh conflict
            account.UpdatedAt = DateTime.UtcNow;
            
            // GenericRepository.UpdateAsync tự động SaveChanges() và Attach()
            // Nên ta có thể bỏ SaveChangesAsync() sau đó.
            await unitOfWork.Accounts.UpdateAsync(account);

            _logger.LogInformation(
                "Successfully updated role for Account {OwnerId}: {OldRoleId} → Seller ({SellerRoleId}) after creating Shop '{ShopName}'",
                shopEvent.OwnerId,
                oldRoleId,
                sellerRole.RoleId,
                shopEvent.ShopName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating role to Seller for OwnerId={OwnerId} after shop creation",
                shopEvent.OwnerId
            );
        }
    }
}
