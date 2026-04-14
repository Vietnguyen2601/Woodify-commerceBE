namespace OrderService.Application.DTOs;

/// <summary>
/// Payload for SignalR after shipment status change (and optional order row sync).
/// </summary>
public class OrderShipmentRealtimePayload
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }

    public string ShipmentPreviousStatus { get; set; } = string.Empty;
    public string ShipmentNewStatus { get; set; } = string.Empty;

    /// <summary>Order status before applying shipment-driven update.</summary>
    public string OrderPreviousStatus { get; set; } = string.Empty;

    /// <summary>Order status after sync (same as before if no DB update).</summary>
    public string OrderNewStatus { get; set; } = string.Empty;

    public bool OrderRowUpdated { get; set; }
    public DateTime OccurredAt { get; set; }
}
