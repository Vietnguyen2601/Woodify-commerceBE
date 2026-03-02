namespace ShipmentService.Domain.Enums;

/// <summary>
/// Loại kiện hàng theo kích thước/trọng lượng
/// </summary>
public enum BulkyType
{
    /// <summary>Hàng thông thường</summary>
    Normal,

    /// <summary>Hàng cồng kềnh</summary>
    Bulky,

    /// <summary>Hàng siêu cồng kềnh</summary>
    SuperBulky
}
