using Microsoft.Extensions.Logging;
using Shared.Constants;

namespace ShipmentService.Infrastructure.Services;

public class MockShippingFeeCalculator : IShippingFeeCalculator
{
    private readonly ILogger<MockShippingFeeCalculator> _logger;

    public MockShippingFeeCalculator(ILogger<MockShippingFeeCalculator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sanitize a string before logging to prevent log forging via control characters.
    /// Removes CR/LF and other non-printable control characters.
    /// </summary>
    private static string SanitizeForLog(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        Span<char> buffer = stackalloc char[input.Length];
        int idx = 0;
        foreach (char c in input)
        {
            // Keep printable characters; drop control chars like \r, \n, etc.
            if (!char.IsControl(c) || c == '\t')
            {
                buffer[idx++] = c;
            }
        }

        return new string(buffer.Slice(0, idx));
    }

    public async Task<ShippingFeeResult> CalculateAsync(int serviceId, int weightGrams)
    {
        // MOCK SHIPPING FEE CALCULATION sử dụng ShippingServiceConstants
        // Determine bucket type dựa trên weight
        string bucketType = ShippingServiceConstants.GetBucketType(weightGrams);

        // Get base fee từ constants
        long baseFee = ShippingServiceConstants.GetBaseFee(serviceId, bucketType);

        // Add weight-based surcharge: mỗi 500g thêm 2,000 VND
        long weightSurcharge = (weightGrams / ShippingServiceConstants.WEIGHT_SURCHARGE_UNIT) * ShippingServiceConstants.WEIGHT_SURCHARGE_PER_UNIT;
        long totalFee = baseFee + weightSurcharge;

        _logger.LogInformation(
            "📦 Mock Shipping Fee: service_id={ServiceId}, weight={Weight}g, bucket={Bucket} → base={Base}đ + surcharge={Surcharge}đ = {Total}đ",
            serviceId, weightGrams, bucketType, baseFee, weightSurcharge, totalFee);

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
        // Use standardized service code mapping from constants
        int serviceId = ShippingServiceConstants.GetServiceId(providerServiceCode);

        if (serviceId == 3) // Default (STANDARD)
        {
            var safeCodeForLog = SanitizeForLog(providerServiceCode);
            _logger.LogWarning(
                "Không tìm thấy mapping service_id cho code '{Code}', dùng default STANDARD (service_id=3).",
                safeCodeForLog);
        }

        return serviceId;
    }
}
