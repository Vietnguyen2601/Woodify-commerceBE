using Microsoft.AspNetCore.SignalR;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.APIService.Hubs;

/// <summary>
/// SignalR Hub để push real-time metrics tới connected clients
/// Client kết nối tại: /admin-dashboard-hub
/// 
/// Events:
/// - ReceiveTodayMetrics: Nhận real-time metrics hôm nay
/// </summary>
public class DashboardHub : Hub
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(IDashboardService dashboardService, ILogger<DashboardHub> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gọi khi client kết nối đến hub
    /// Gửi ngay metrics hiện tại cho client
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        try
        {
            _logger.LogInformation($"Client {Context.ConnectionId} connected to DashboardHub");

            // Lấy metrics hiện tại
            var metrics = await _dashboardService.GetTodayMetricsAsync();

            // Gửi cho client connect vừa xong
            await Clients.Caller.SendAsync("ReceiveTodayMetrics", metrics);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DashboardHub OnConnectedAsync");
            throw;
        }
    }

    /// <summary>
    /// Gọi khi client disconnect
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client {Context.ConnectionId} disconnected from DashboardHub");
        if (exception != null)
        {
            _logger.LogError(exception, "Disconnection error");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Để client có thể request metrics trực tiếp nếu cần
    /// </summary>
    public async Task RequestLatestMetrics()
    {
        try
        {
            var metrics = await _dashboardService.GetTodayMetricsAsync();
            await Clients.Caller.SendAsync("ReceiveTodayMetrics", metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RequestLatestMetrics");
            await Clients.Caller.SendAsync("Error", new { message = "Failed to get metrics" });
        }
    }
}
