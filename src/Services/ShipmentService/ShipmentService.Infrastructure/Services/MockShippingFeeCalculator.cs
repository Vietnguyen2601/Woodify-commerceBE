using Microsoft.Extensions.Logging;

namespace ShipmentService.Infrastructure.Services;

public class MockShippingFeeCalculator : IShippingFeeCalculator
{
    private readonly ILogger<MockShippingFeeCalculator> _logger;
    private readonly Dictionary<string, int> _serviceMapping;

    private static readonly Dictionary<string, int> DefaultServiceMapping = new()
    {
        ["ECO"] = 5,
        ["STD"] = 5,
        ["EXP"] = 2,
        ["SUP"] = 1,
    };

    public MockShippingFeeCalculator(ILogger<MockShippingFeeCalculator> logger)
    {
        _logger = logger;
        _serviceMapping = new Dictionary<string, int>(DefaultServiceMapping);
    }

    public async Task<ShippingFeeResult> CalculateAsync(int serviceId, int weightGrams)
    {
        // MOCK SHIPPING FEE CALCULATION (No external API, no district/ward logic)
        // Calculate base fee by service type
        long baseFee = serviceId switch
        {
            1 => 35000,  // SUP (Super Express) - 35k VND
            2 => 28000,  // EXP (Express) - 28k VND
            5 => 20000,  // STD/ECO (Standard/Economy) - 20k VND
            _ => 25000   // Default - 25k VND
        };

        // Add weight-based surcharge (2k VND per 500g)
        long weightSurcharge = (weightGrams / 500) * 2000;
        long totalFee = baseFee + weightSurcharge;

        _logger.LogInformation(
            "📦 Mock Shipping Fee: service_id={ServiceId}, weight={Weight}g → base={Base}đ + surcharge={Surcharge}đ = {Total}đ",
            serviceId, weightGrams, baseFee, weightSurcharge, totalFee);

        await Task.CompletedTask; // Keep async signature

        return new ShippingFeeResult
        {
            Total = totalFee,
            ServiceFee = totalFee,
            InsuranceFee = 0,
            CouponValue = 0
        };
    }

    public int MapServiceCode(string providerServiceCode)
    {
        if (_serviceMapping.TryGetValue(providerServiceCode.ToUpperInvariant(), out int id))
            return id;

        _logger.LogWarning("Không tìm thấy mapping service_id cho code '{Code}', dùng default 5.", providerServiceCode);
        return 5; // Standard fallback
    }
}
