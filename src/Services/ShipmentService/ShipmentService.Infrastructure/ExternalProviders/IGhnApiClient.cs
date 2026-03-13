namespace ShipmentService.Infrastructure.ExternalProviders;

public interface IGhnApiClient
{
    Task<GhnFeeResponse> GetFeeAsync(GhnFeeRequest request);

    /// <summary>Pickup format: "{from_district_id}" (số nguyên), e.g. "1442".</summary>
    GhnAddressInfo? ResolvePickupAddress(string pickupAddressId);

    /// <summary>Delivery format: "{to_district_id}_{to_ward_code}", e.g. "1444_21105".</summary>
    GhnAddressInfo? ResolveDeliveryAddress(string deliveryAddressId);

    /// <summary>Maps a ProviderService code (ECO/STD/EXP/SUP) to its GHN service_id integer.</summary>
    int MapServiceCode(string providerServiceCode);
}
