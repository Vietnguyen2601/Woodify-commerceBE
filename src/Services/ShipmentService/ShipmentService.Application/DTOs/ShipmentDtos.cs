namespace ShipmentService.Application.DTOs;

// ── Shipment ──────────────────────────────────────────────────────────────────

/// <summary>
/// Create shipment (seller). Server generates <see cref="ShipmentDto.TrackingNumber"/>.
/// Addresses / service code default from order cache + shop cache when omitted.
/// </summary>
public class CreateShipmentDto
{
    public Guid ShopId { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>Optional; otherwise from order snapshot (checkout).</summary>
    public string? ProviderServiceCode { get; set; }

    /// <summary>Optional; otherwise shop default pickup address.</summary>
    public string? PickupAddress { get; set; }

    /// <summary>Optional; otherwise order delivery address.</summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>Override total weight (grams). Otherwise from order.created cache.</summary>
    public double? TotalWeightGrams { get; set; }

    /// <summary>Optional; otherwise derived from weight (NORMAL / BULKY / SUPER_BULKY).</summary>
    public string? BulkyType { get; set; }

    /// <summary>Seller override in VND. If null, computed via shared shipping rules (or free-shipping threshold).</summary>
    public long? FinalShippingFeeVnd { get; set; }

    /// <summary>When true, forces fee to 0 and marks free shipping.</summary>
    public bool? ForceFreeShipping { get; set; }

    /// <summary>Seller-scheduled pickup window (optional).</summary>
    public DateTime? PickupScheduledAt { get; set; }

    /// <summary>Optional ETA override; otherwise from provider service SLA + order time.</summary>
    public DateTime? DeliveryEstimatedAt { get; set; }
}

/// <summary>Partial update (seller): any non-null field is applied.</summary>
public class UpdateShipmentDto
{
    public string? TrackingNumber { get; set; }
    public Guid? ProviderServiceId { get; set; }
    public string? PickupAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public double? TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; }
    public long? FinalShippingFeeVnd { get; set; }
    public bool? IsFreeShipping { get; set; }
    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? CancelReason { get; set; }
}

public class UpdateShipmentStatusDto
{
    /// <summary>One of the shipment status codes (case-insensitive; stored uppercase).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Required when moving <b>to</b> <c>DELIVERY_FAILED</c> unless already set on the shipment (via PATCH).</summary>
    public string? FailureReason { get; set; }

    /// <summary>Required when moving <b>to</b> <c>CANCELLED</c> unless already set on the shipment.</summary>
    public string? CancelReason { get; set; }
}

public class UpdateShipmentPickupDto
{
    /// <summary>When null, server uses UTC now.</summary>
    public DateTime? PickedUpAt { get; set; }
}

public class ShipmentDto
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    /// <summary>Seller shop that created this shipment; use with <c>GET .../by-shop/{shopId}</c>.</summary>
    public Guid ShopId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ProviderServiceCode { get; set; }
    public string? ShippingProviderName { get; set; }
    public string? PickupAddress { get; set; }
    public string? DeliveryAddress { get; set; }
    public double TotalWeightGrams { get; set; }
    public string? BulkyType { get; set; }
    public long FinalShippingFeeVnd { get; set; }
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
