namespace ShipmentService.Application.DTOs;

/// <summary>Kết quả tính phí vận chuyển.</summary>
public class ShippingFeeResult
{
    public long Total { get; set; }
    public long ServiceFee { get; set; }
    public long InsuranceFee { get; set; }
    public long CouponValue { get; set; }
}
