namespace ShipmentService.Domain.Enums;

/// <summary>
/// Tốc độ dịch vụ giao hàng của provider
/// </summary>
public enum SpeedLevel
{
    /// <summary>Tiết kiệm</summary>
    Economy,

    /// <summary>Tiêu chuẩn</summary>
    Standard,

    /// <summary>Nhanh</summary>
    Express,

    /// <summary>Siêu nhanh</summary>
    SuperExpress
}
