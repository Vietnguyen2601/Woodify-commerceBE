using OrderService.Domain.Entities;

namespace OrderService.Application.Shipping;

/// <summary>
/// Maps ShipmentService status strings to <see cref="OrderStatus"/> for aggregate sync.
/// </summary>
public static class ShipmentToOrderStatusMapper
{
    public static OrderStatus? MapOrderStatus(string? shipmentStatus)
    {
        if (string.IsNullOrWhiteSpace(shipmentStatus))
            return null;

        return Normalize(shipmentStatus) switch
        {
            "DRAFT" => null,
            "PENDING" => OrderStatus.CONFIRMED,
            "PICKUP_SCHEDULED" => OrderStatus.CONFIRMED,
            "PICKED_UP" => OrderStatus.SHIPPED,
            "IN_TRANSIT" => OrderStatus.SHIPPED,
            "OUT_FOR_DELIVERY" => OrderStatus.SHIPPED,
            "DELIVERED" => OrderStatus.DELIVERED,
            "DELIVERY_FAILED" => OrderStatus.PROCESSING,
            "RETURNING" => OrderStatus.REFUNDING,
            "RETURNED" => OrderStatus.REFUNDED,
            "CANCELLED" => OrderStatus.CANCELLED,
            _ => null
        };
    }

    private static string Normalize(string s) => s.Trim().ToUpperInvariant();
}
