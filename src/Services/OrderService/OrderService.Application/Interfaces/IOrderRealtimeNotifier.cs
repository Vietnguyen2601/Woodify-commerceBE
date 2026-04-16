using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Push order/shipment status updates to SignalR clients (implemented in APIService).
/// </summary>
public interface IOrderRealtimeNotifier
{
    Task NotifyOrderShipmentStatusAsync(OrderShipmentRealtimePayload payload, CancellationToken cancellationToken = default);
}
