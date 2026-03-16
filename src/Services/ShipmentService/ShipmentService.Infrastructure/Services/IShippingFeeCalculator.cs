namespace ShipmentService.Infrastructure.Services;

/// <summary>Interface for calculating shipping fees (mock implementation)</summary>
public interface IShippingFeeCalculator
{
    /// <summary>Calculate shipping fee based on service type and weight</summary>
    Task<ShippingFeeResult> CalculateAsync(int serviceId, int weightGrams);

    /// <summary>Maps a ProviderService code (ECO/STD/EXP/SUP) to its service_id integer.</summary>
    int MapServiceCode(string providerServiceCode);
}

public class ShippingFeeResult
{
    public long Total { get; set; }
    public long ServiceFee { get; set; }
    public long InsuranceFee { get; set; }
    public long CouponValue { get; set; }
}
