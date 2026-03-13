namespace ShipmentService.Application.DTOs;

/// <summary>Request DTO for the shipping fee preview endpoint.</summary>
public class ShippingFeePreviewRequest
{
    public Guid ShopId { get; set; }
    public string ProviderServiceCode { get; set; } = string.Empty;
    public double TotalWeightGrams { get; set; }

    /// <summary>NORMAL | BULKY (+20%) | SUPER_BULKY (+50%)</summary>
    public string BulkyType { get; set; } = "NORMAL";

    /// <summary>GHN pickup address: "{from_district_id}" e.g. "1442"</summary>
    public string PickupAddressId { get; set; } = string.Empty;

    /// <summary>GHN delivery address: "{district_id}_{ward_code}" e.g. "1444_21105"</summary>
    public string DeliveryAddressId { get; set; } = string.Empty;

    /// <summary>Order subtotal in cents - used to check freeship threshold.</summary>
    public long? SubtotalCents { get; set; }
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