namespace ShipmentService.Domain.Enums;

/// <summary>
/// Trạng thái vận chuyển
/// </summary>
public enum ShipmentStatus
{
    DRAFT,
    PENDING,
    PICKUP_SCHEDULED,
    PICKED_UP,
    IN_TRANSIT,
    OUT_FOR_DELIVERY,
    DELIVERED,
    DELIVERY_FAILED,
    RETURNING,
    RETURNED,
    CANCELLED
}

/// <summary>
/// Loại kiện hàng cồng kềnh
/// </summary>
public enum BulkyType
{
    NORMAL,
    BULKY,
    SUPER_BULKY
}

/// <summary>
/// Mã gói dịch vụ vận chuyển
/// </summary>
public enum ProviderServiceCode
{
    ECO,
    STD,
    EXP,
    SUP
}

/// <summary>
/// Tốc độ vận chuyển
/// </summary>
public enum SpeedLevel
{
    ECONOMY,
    STANDARD,
    EXPRESS,
    SUPER_EXPRESS
}
