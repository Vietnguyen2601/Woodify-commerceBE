using ShipmentService.Application.DTOs;

namespace ShipmentService.Application.Interfaces;

public interface IShippingFeeCalculator
{
    Task<ShippingFeeResult> CalculateAsync(int serviceId, int weightGrams);
    int MapServiceCode(string providerServiceCode);
}
