namespace ShipmentService.Application.DTOs;

// ── Shipment ──────────────────────────────────────────────────────────────────

public class CreateShipmentDto
{
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string? PickupAddressId { get; set; }
    public string? DeliveryAddressId { get; set; }
    public double TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; } // NORMAL, BULKY, SUPER_BULKY
    public long FinalShippingFeeCents { get; set; }
    public bool IsFreeShipping { get; set; } = false;
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }
}

public class UpdateShipmentDto
{
    public string? TrackingNumber { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string? PickupAddressId { get; set; }
    public string? DeliveryAddressId { get; set; }
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
    public string? FailureReason { get; set; }
    public string? CancelReason { get; set; }
}

public class ShipmentDto
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public string? TrackingNumber { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string? ProviderServiceName { get; set; }
    public string? PickupAddressId { get; set; }
    public string? DeliveryAddressId { get; set; }
    public double TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; }
    public long FinalShippingFeeCents { get; set; }
    public bool IsFreeShipping { get; set; }
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
