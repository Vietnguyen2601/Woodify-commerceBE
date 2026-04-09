using Microsoft.AspNetCore.SignalR;
using OrderService.APIService.Hubs;
using OrderService.Application.Interfaces;

namespace OrderService.APIService.Services;

/// <summary>
/// Background service để push real-time metrics lên connected SignalR clients
/// Chạy liên tục, update metrics mỗi 5 giây
/// 
/// Được khởi động tự động khi app starts
/// Dừng khi app stops
/// </summary>
public class MetricsPublisherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<MetricsPublisherService> _logger;

    /// <summary>
    /// Interval để broadcast metrics (milliseconds)
    /// Được set thành 5 giây (5000 ms)
    /// </summary>
    private const int UpdateIntervalMs = 5000;

    public MetricsPublisherService(
        IServiceProvider serviceProvider,
        IHubContext<DashboardHub> hubContext,
        ILogger<MetricsPublisherService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Main execution loop cho background service
    /// Chạy mỗi {UpdateIntervalMs} ms để push metrics tới clients
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MetricsPublisherService starting. Updates every {IntervalMs}ms", UpdateIntervalMs);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create new scope để lấy fresh instance của services
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dashboardService = scope.ServiceProvider
                            .GetRequiredService<IDashboardService>();

                        // Lấy metrics hiện tại
                        var metrics = await dashboardService.GetTodayMetricsAsync();

                        // Broadcast tới TẤT CẢ connected clients
                        await _hubContext.Clients.All
                            .SendAsync("ReceiveTodayMetrics", metrics, cancellationToken: stoppingToken);

                        _logger.LogDebug("Metrics broadcasted to {ConnectedCount} clients at {Timestamp}",
                            "all", metrics.Timestamp);
                    }
                }
                catch (Exception ex)
                {
                    // Log error nhưng không stop background service
                    _logger.LogError(ex, "Error while publishing metrics");
                }

                // Chờ trước khi update lần tiếp theo
                await Task.Delay(UpdateIntervalMs, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MetricsPublisherService stopping (cancellation requested)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MetricsPublisherService encountered an error");
            throw;
        }
    }

    /// <summary>
    /// Gọi khi service dừng
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MetricsPublisherService stopped");
        await base.StopAsync(cancellationToken);
    }
}
