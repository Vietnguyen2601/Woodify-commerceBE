using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Events;
using Shared.Messaging;

namespace PaymentService.Application.Consumers;

/// <summary>
/// Background Service lắng nghe AccountCreatedEvent từ RabbitMQ
/// Nhận event "account.created" từ IdentityService
/// Tự động tạo wallet cho user mới
/// </summary>
public class AccountCreatedConsumer : BackgroundService
{
    private readonly ILogger<AccountCreatedConsumer> _logger;
    private readonly RabbitMQConsumer _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AccountCreatedConsumer(
        ILogger<AccountCreatedConsumer> logger,
        RabbitMQConsumer consumer,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("✅ AccountCreatedConsumer started. Listening for 'account.created' events...");

            // Subscribe để nhận message từ queue "account.created"
            _consumer.Subscribe<AccountCreatedEvent>("account.created", (accountEvent) =>
            {
                _logger.LogInformation(
                    "[RabbitMQ] Received account.created event: AccountId={AccountId}, Email={Email}, Username={Username}",
                    accountEvent.AccountId,
                    accountEvent.Email,
                    accountEvent.Username
                );

                // 🔔 Xử lý business logic khi nhận được event - Tạo wallet tự động
                ProcessAccountCreatedEventAsync(accountEvent).Wait();
            });

            // Keep the task running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AccountCreatedConsumer is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in AccountCreatedConsumer");
            throw;
        }
    }

    private async Task ProcessAccountCreatedEventAsync(AccountCreatedEvent accountEvent)
    {
        try
        {
            _logger.LogInformation("Processing account creation for AccountId: {AccountId}", accountEvent.AccountId);

            // Tạo scope để có thể sử dụng scoped service
            using var scope = _serviceScopeFactory.CreateScope();
            var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

            // Tạo wallet mới với currency mặc định là VND
            var createWalletRequest = new CreateWalletRequest
            {
                AccountId = accountEvent.AccountId,
                Currency = "VND"  // Mặc định là VND
            };

            var result = await walletService.CreateWalletAsync(createWalletRequest);

            if (result.Status == 201)
            {
                _logger.LogInformation(
                    "✅ Wallet created successfully for AccountId: {AccountId}",
                    accountEvent.AccountId
                );
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Failed to create wallet for AccountId: {AccountId}. Details: {Message}",
                    accountEvent.AccountId,
                    result.Message
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating wallet for AccountId: {AccountId}", accountEvent.AccountId);
        }
    }
}
