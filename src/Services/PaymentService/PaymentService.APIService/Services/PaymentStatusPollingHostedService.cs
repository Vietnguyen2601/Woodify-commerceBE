using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;

namespace PaymentService.APIService.Services;

/// <summary>
/// Background Service để polling payment status từ PayOS.
/// Chỉ poll khi được trigger (sau khi user tạo payment link).
/// Poll mỗi 5 giây cho đến khi không còn payment Processing nào (tối đa 1 giờ).
/// </summary>
public class PaymentStatusPollingHostedService : BackgroundService, IPaymentPollingTrigger
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentStatusPollingHostedService> _logger;
    private readonly SemaphoreSlim _signal = new(0, 1);
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _maxSessionDuration = TimeSpan.FromHours(1);
    private readonly TimeSpan _paymentExpiry = TimeSpan.FromMinutes(15);

    public PaymentStatusPollingHostedService(
        IServiceProvider serviceProvider,
        ILogger<PaymentStatusPollingHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Trigger()
    {
        if (_signal.CurrentCount == 0)
        {
            _signal.Release();
            _logger.LogInformation("Polling session triggered");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Status Polling Hosted Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await _signal.WaitAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }

            _logger.LogInformation("Polling session started - will poll until no processing payments remain");

            var sessionStartTime = DateTime.UtcNow;
            var deadline = sessionStartTime.Add(_maxSessionDuration);
            while (DateTime.UtcNow < deadline && !stoppingToken.IsCancellationRequested)
            {
                bool hasRemaining;
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    hasRemaining = await PollAsync(scope.ServiceProvider, sessionStartTime);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in polling loop");
                    hasRemaining = true; // giữ retry khi lỗi tạm thời
                }

                if (!hasRemaining)
                {
                    _logger.LogInformation("No more processing payments. Polling session ended.");
                    break;
                }

                try { await Task.Delay(_pollingInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        _logger.LogInformation("Payment Status Polling Hosted Service stopped");
    }

    // Trả về true nếu vẫn còn payment tạo trong session hiện tại đang PENDING
    // Payments cũ (tạo trước session) vẫn được expire nhưng không giữ session sống
    private async Task<bool> PollAsync(IServiceProvider services, DateTime sessionStartTime)
    {
        var paymentRepo = services.GetRequiredService<IPaymentRepository>();
        var payOsService = services.GetRequiredService<IPayOsService>();
        var webhookHandler = services.GetRequiredService<IPayOsWebhookHandler>();

        var processingPayments = (await paymentRepo.GetAllProcessingAsync()).ToList();
        if (processingPayments.Count == 0)
        {
            _logger.LogDebug("No processing payments found");
            return false;
        }

        _logger.LogDebug("Polling {Count} processing payments", processingPayments.Count);

        int stillPendingCount = 0;

        foreach (var payment in processingPayments)
        {
            if (string.IsNullOrEmpty(payment.ProviderPaymentId) || payment.Provider != "PAYOS") continue;
            if (!long.TryParse(payment.ProviderPaymentId, out var orderCode)) continue;

            // Auto-expire: payment PENDING quá 15 phút → mark Failed, không call PayOS nữa
            if (DateTime.UtcNow - payment.CreatedAt > _paymentExpiry)
            {
                _logger.LogWarning("Payment expired (>{Expiry}min). Marking as Failed. PaymentId: {PaymentId}, OrderCode: {OrderCode}",
                    _paymentExpiry.TotalMinutes, payment.PaymentId, orderCode);
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await paymentRepo.UpdateAsync(payment);
                continue;
            }

            try
            {
                var info = await payOsService.GetPaymentInfoAsync(orderCode);
                var status = info?.Status?.ToUpper();

                if (status == "PAID")
                {
                    _logger.LogInformation("Payment PAID detected, crediting wallet. PaymentId: {PaymentId}", payment.PaymentId);
                    await webhookHandler.HandleWebhookAsync(new PayOsWebhookData
                    {
                        Data = new PayOsWebhookPaymentData
                        {
                            OrderCode = orderCode,
                            Status = "PAID",
                            Description = "Auto-detected from polling",
                            Amount = payment.AmountVnd > int.MaxValue
                                ? int.MaxValue
                                : (int)payment.AmountVnd,
                            Reference = ""
                        },
                        Signature = "auto-polling"
                    });
                    // Đã resolve → không count vào stillPending
                }
                else if (status == "CANCELLED")
                {
                    _logger.LogInformation("Payment CANCELLED. PaymentId: {PaymentId}", payment.PaymentId);
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await paymentRepo.UpdateAsync(payment);
                    // Đã resolve → không count vào stillPending
                }
                else
                {
                    // PayOS v��n PENDING (hoặc status khác) — tiếp tục poll
                    stillPendingCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment {PaymentId}", payment.PaymentId);
                stillPendingCount++; // giữ retry khi lỗi tạm thời
            }
        }

        return stillPendingCount > 0;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment Status Polling Hosted Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
