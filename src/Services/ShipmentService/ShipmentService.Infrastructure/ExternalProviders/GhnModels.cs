namespace ShipmentService.Infrastructure.ExternalProviders;

public class GhnFeeRequest
{
    public int ServiceId { get; set; }
    public int FromDistrictId { get; set; }
    public int ToDistrictId { get; set; }
    public string ToWardCode { get; set; } = string.Empty;
    public int Weight { get; set; }
    public long? InsuranceValue { get; set; }
}

public class GhnFeeResponse
{
    public long Total { get; set; }
    public long ServiceFee { get; set; }
    public long InsuranceFee { get; set; }
    public long CouponValue { get; set; }
}

/// <summary>Thông tin địa chỉ GHN sau khi phân giải từ address ID string.</summary>
public class GhnAddressInfo
{
    public int DistrictId { get; set; }
    public string WardCode { get; set; } = string.Empty;
}
