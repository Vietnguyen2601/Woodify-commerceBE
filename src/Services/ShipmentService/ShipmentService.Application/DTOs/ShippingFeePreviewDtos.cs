namespace ShipmentService.Application.DTOs;

/// <summary>Request DTO for the shipping fee preview endpoint.</summary>
public class ShippingFeePreviewRequest
{
    public Guid ShopId { get; set; }
    public Guid OrderId { get; set; }
    public string? ProviderServiceCode { get; set; }

    /// <summary>Pickup address (optional - not used in mock calculation)</summary>
    public string? PickupAddress { get; set; }

    /// <summary>Delivery address (optional - not used in mock calculation)</summary>
    public string? DeliveryAddress { get; set; }

    public Guid? VoucherId { get; set; }
}

/// <summary>Response with fully broken-down shipping fee for the selected service.</summary>
public class ShippingFeePreviewResponse
{
    public string ProviderServiceCode { get; set; } = string.Empty;
    public string? ServiceName { get; set; }

    /// <summary>Raw fee from the shipping provider (cents).</summary>
    public long BaseFeeCents { get; set; }

    /// <summary>Bulky surcharge applied on top of BaseFeeCents (cents).</summary>
    public long SurchargeCents { get; set; }

    /// <summary>Multiplier from ProviderService.MultiplierFee - platform margin.</summary>
    public double MultiplierFee { get; set; }

    /// <summary>final = Round((base + surcharge) * multiplier). 0 when IsFreeShipping.</summary>
    public long FinalShippingFeeCents { get; set; }

    public bool IsFreeShipping { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public string Message { get; set; } = string.Empty;
}