namespace ShipmentService.Application.DTOs;

// ── Shipment ──────────────────────────────────────────────────────────────────

public class CreateShipmentDto
{
    public Guid ShopId { get; set; }
    public Guid OrderId { get; set; }
    public string? ProviderServiceCode { get; set; }     // e.g. "STD" — resolved to ProviderService internally
    public string? PickupAddress { get; set; }          // optional: auto-filled from shop's DefaultPickupAddress
    public string? DeliveryAddress { get; set; }        // optional: auto-filled from order's DeliveryAddressId
}

public class UpdateShipmentDto
{
    public string? TrackingNumber { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string? PickupAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public double? TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; }
    public long? FinalShippingFeeCents { get; set; }
    public bool? IsFreeShipping { get; set; }
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }
}

public class UpdateShipmentStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateShipmentPickupDto
{
    public DateTime? PickedUpAt { get; set; }
}

public class ShipmentDto
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ProviderServiceCode { get; set; }
    public string? ShippingProviderName { get; set; }
    public string? PickupAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public double TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; }
    public long FinalShippingFeeCents { get; set; }
    public bool IsFreeShipping { get; set; }
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
