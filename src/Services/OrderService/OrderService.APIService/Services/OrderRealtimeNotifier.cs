using Microsoft.AspNetCore.SignalR;
using OrderService.APIService.Hubs;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;

namespace OrderService.APIService.Services;

/// <summary>
/// Pushes <see cref="OrderShipmentRealtimePayload"/> to SignalR groups (order, shop, buyer).
/// </summary>
public class OrderRealtimeNotifier : IOrderRealtimeNotifier
{
    private readonly IHubContext<OrdersHub> _hubContext;
    private readonly ILogger<OrderRealtimeNotifier> _logger;

    public OrderRealtimeNotifier(
        IHubContext<OrdersHub> hubContext,
        ILogger<OrderRealtimeNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderShipmentStatusAsync(
        OrderShipmentRealtimePayload payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oid = payload.OrderId.ToString();
            var sid = payload.ShopId.ToString();
            var aid = payload.AccountId.ToString();

            await _hubContext.Clients
                .Group(OrdersHub.OrderGroupName(oid))
                .SendAsync(OrdersHub.OrderShipmentStatusUpdated, payload, cancellationToken);

            await _hubContext.Clients
                .Group(OrdersHub.ShopGroupName(sid))
                .SendAsync(OrdersHub.OrderShipmentStatusUpdated, payload, cancellationToken);

            if (payload.AccountId != Guid.Empty)
            {
                await _hubContext.Clients
                    .Group(OrdersHub.AccountGroupName(aid))
                    .SendAsync(OrdersHub.OrderShipmentStatusUpdated, payload, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "SignalR notify failed for Order {OrderId}",
                payload.OrderId);
        }
    }
}
