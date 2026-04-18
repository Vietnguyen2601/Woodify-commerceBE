using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using Shared.Events;
using Shared.Messaging;

namespace PaymentService.Application.Consumers;

/// <summary>
/// Ghi có / hoàn tác ví seller từ OrderService (order.events).
/// </summary>
public class SellerSettlementEventsConsumer : BackgroundService
{
    private readonly ILogger<SellerSettlementEventsConsumer> _logger;
    private readonly RabbitMQConsumer _consumer;
    private readonly IServiceScopeFactory _scopeFactory;

    public SellerSettlementEventsConsumer(
        ILogger<SellerSettlementEventsConsumer> logger,
        RabbitMQConsumer consumer,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _consumer = consumer;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _consumer.Subscribe<OrderSellerNetEligibleEvent>(
                "paymentservice.order.seller.net.eligible",
                "order.events",
                "order.seller.net.eligible",
                HandleEligibleAsync);

            _consumer.Subscribe<OrderSellerNetReversedEvent>(
                "paymentservice.order.seller.net.reversed",
                "order.events",
                "order.seller.net.reversed",
                HandleReversedAsync);

            _logger.LogInformation("SellerSettlementEventsConsumer listening (seller net eligible + reversal)");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SellerSettlementEventsConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SellerSettlementEventsConsumer failed");
            throw;
        }
    }

    private async Task HandleEligibleAsync(OrderSellerNetEligibleEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
            await repo.ApplySellerOrderNetCreditAsync(evt, CancellationToken.None);
            _logger.LogInformation(
                "Seller net credited OrderId={OrderId} Seller={Seller} Net={Net}",
                evt.OrderId, evt.SellerAccountId, evt.NetAmountVnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HandleEligibleAsync OrderId={OrderId}", evt.OrderId);
            throw;
        }
    }

    private async Task HandleReversedAsync(OrderSellerNetReversedEvent evt)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
            await repo.ApplySellerOrderNetReversalAsync(evt, CancellationToken.None);
            _logger.LogInformation(
                "Seller net reversed OrderId={OrderId} Seller={Seller} Net={Net}",
                evt.OrderId, evt.SellerAccountId, evt.NetAmountVnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HandleReversedAsync OrderId={OrderId}", evt.OrderId);
            throw;
        }
    }
}
