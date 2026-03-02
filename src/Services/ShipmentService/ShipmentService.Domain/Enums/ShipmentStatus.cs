namespace ShipmentService.Domain.Enums;

/// <summary>
/// Trạng thái vận chuyển của lô hàng
/// </summary>
public enum ShipmentStatus
{
    /// <summary>Bản nháp, chưa xác nhận</summary>
    Draft,

    /// <summary>Chờ xử lý</summary>
    Pending,

    /// <summary>Đã lên lịch lấy hàng</summary>
    PickupScheduled,

    /// <summary>Đã lấy hàng</summary>
    PickedUp,

    /// <summary>Đang vận chuyển</summary>
    InTransit,

    /// <summary>Đang giao đến người nhận</summary>
    OutForDelivery,

    /// <summary>Đã giao thành công</summary>
    Delivered,

    /// <summary>Giao hàng thất bại</summary>
    DeliveryFailed,

    /// <summary>Đang hoàn hàng</summary>
    Returning,

    /// <summary>Đã hoàn hàng</summary>
    Returned,

    /// <summary>Đã hủy</summary>
    Cancelled
}
