using Microsoft.Extensions.Logging;
using Shared.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;

namespace ShipmentService.Application.Services;

public class ShippingFeeCalculator : IShippingFeeCalculator
{
    private readonly ILogger<ShippingFeeCalculator> _logger;

    public ShippingFeeCalculator(ILogger<ShippingFeeCalculator> logger)
    {
        _logger = logger;
    }

    public async Task<ShippingFeeResult> CalculateAsync(int serviceId, int weightGrams)
    {
        string bucketType = ShippingServiceConstants.GetBucketType(weightGrams);
        long baseFee = ShippingServiceConstants.GetBaseFee(serviceId, bucketType);
        long weightSurcharge = (weightGrams / ShippingServiceConstants.WEIGHT_SURCHARGE_UNIT)
            * ShippingServiceConstants.WEIGHT_SURCHARGE_PER_UNIT;
        long totalFee = baseFee + weightSurcharge;

        _logger.LogInformation(
            "Shipping fee: service_id={ServiceId}, weight={Weight}g, bucket={Bucket} → base={Base}đ + surcharge={Surcharge}đ = {Total}đ",
            serviceId, weightGrams, bucketType, baseFee, weightSurcharge, totalFee);

        await Task.CompletedTask;

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
        int serviceId = ShippingServiceConstants.GetServiceId(providerServiceCode);

        if (serviceId == 3)
        {
            _logger.LogWarning(
                "Không tìm thấy mapping service_id cho code '{Code}', dùng default STANDARD (service_id=3).",
                providerServiceCode ?? string.Empty);
        }

        return serviceId;
    }
}
