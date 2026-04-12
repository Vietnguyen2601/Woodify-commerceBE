namespace Shared.Events;

/// <summary>
/// Events dành riêng cho ShopService Dashboard Metrics
/// Được publish bởi OrderService khi order trạng thái thay đổi
/// Consumed bởi ShopService để update realtime dashboard metrics
/// </summary>

/// <summary>
/// Event khi Order status thay đổi
/// OrderService publish → ShopService consume (update dashboard cache)
/// Exchange: "order.events" / Routing key: "order.status.changed"
/// Sử dụng để cập nhật:
/// - TaskBoard (AwaitingPickup, ReadyToShip, Returns counts)
/// - KPI Metrics (doanh thu, conversion rate)
/// - Orders breakdown
/// </summary>
public class OrderStatusChangedEvent
{
    /// <summary>ID của order</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID của shop sở hữu order</summary>
    public Guid ShopId { get; set; }

    /// <summary>Status cũ (trước khi thay đổi)</summary>
    public string PreviousStatus { get; set; } = string.Empty;

    /// <summary>Status mới</summary>
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>Tổng số tiền order (VND)</summary>
    public long TotalAmountCents { get; set; }

    /// <summary>Tiền hoa hồng (VND)</summary>
    public long CommissionCents { get; set; }

    /// <summary>Tiền ròng shop nhận được (TotalAmountCents - CommissionCents)</summary>
    public long NetAmountCents { get; set; }

    /// <summary>Thời gian thay đổi status</summary>
    public DateTime StatusChangedAt { get; set; }

    /// <summary>Thời gian tạo order (dùng để track SLA)</summary>
    public DateTime OrderCreatedAt { get; set; }

    /// <summary>Số lượng items trong order</summary>
    public int ItemCount { get; set; } = 0;

    /// <summary>ID phiên bản sản phẩm chính (từ OrderItem đầu tiên)</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }
}

/// <summary>
/// Event khi Order được hoàn thành (completed)
/// OrderService publish → ShopService consume (update revenue metrics)
/// Exchange: "order.events" / Routing key: "order.completed"
/// Sử dụng để:
/// - Cập nhật doanh thu hôm nay (RevenueToday)
/// - Tính conversion rate
/// - Update revenue trend
/// </summary>
public class OrderCompletedEvent
{
    /// <summary>ID của order</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID của shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>Tổng doanh thu (VND, tính bằng cents)</summary>
    public long TotalAmountCents { get; set; }

    /// <summary>Commission rate áp dụng cho order</summary>
    public decimal CommissionRate { get; set; }

    /// <summary>Commission amount (VND, cents)</summary>
    public long CommissionCents { get; set; }

    /// <summary>Ngày hoàn thành</summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>Số lượng items trong order</summary>
    public int ItemCount { get; set; } = 0;

    /// <summary>ID phiên bản sản phẩm chính</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }
}

/// <summary>
/// Event khi Order được tạo mới (cho ShopService Dashboard)
/// OrderService publish → ShopService consume (update order list & metrics)
/// Exchange: "shop.events" / Routing key: "order.created"
/// Sử dụng để:
/// - Cập nhật danh sách order realtime
/// - Cập nhật tổng số order PENDING
/// - Track order theo tháng/năm
/// </summary>
public class OrderCreatedForShopEvent
{
    /// <summary>ID của order</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID của shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>ID của account (buyer)</summary>
    public Guid AccountId { get; set; }

    /// <summary>Tổng tiền order (VND, đơn vị: cents = VND * 10)</summary>
    public long TotalAmountCents { get; set; }

    /// <summary>Tiền hoa hồng (VND, đơn vị: cents)</summary>
    public long CommissionCents { get; set; }

    /// <summary>Tỷ lệ hoa hồng áp dụng</summary>
    public decimal CommissionRate { get; set; }

    /// <summary>Số lượng items trong order</summary>
    public int ItemCount { get; set; }

    /// <summary>ID phiên bản sản phẩm chính (từ OrderItem đầu tiên)</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }

    /// <summary>Địa chỉ giao hàng</summary>
    public string? DeliveryAddress { get; set; }

    /// <summary>Mã dịch vụ vận chuyển</summary>
    public string? ProviderServiceCode { get; set; }

    /// <summary>Ngày tạo order</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Event khi Order bị hủy
/// OrderService publish → ShopService consume (update metrics)
/// Exchange: "order.events" / Routing key: "order.cancelled"
/// Sử dụng để:
/// - Update returns/cancelled count
/// - Adjust revenue calculations
/// </summary>
public class OrderCancelledEvent
{
    /// <summary>ID của order</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID của shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>Lý do hủy</summary>
    public string CancelReason { get; set; } = string.Empty;

    /// <summary>Số tiền bị hủy (VND, cents)</summary>
    public long CancelledAmountCents { get; set; }

    /// <summary>Thời gian hủy</summary>
    public DateTime CancelledAt { get; set; }

    /// <summary>Số lượng items trong order</summary>
    public int ItemCount { get; set; } = 0;

    /// <summary>ID phiên bản sản phẩm chính</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }
}

/// <summary>
/// Event khi Order được hoàn tiền
/// OrderService publish → ShopService consume (update metrics)
/// Exchange: "order.events" / Routing key: "order.refunded"
/// Sử dụng để:
/// - Update returns/refunds count
/// - Adjust revenue calculations
/// - Track SLA violations (refund < 24h)
/// </summary>
public class OrderRefundedEvent
{
    /// <summary>ID của order</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID của shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>Số tiền hoàn lại (VND, cents)</summary>
    public long RefundAmountCents { get; set; }

    /// <summary>Lý do hoàn tiền</summary>
    public string RefundReason { get; set; } = string.Empty;

    /// <summary>Thời gian hoàn tiền</summary>
    public DateTime RefundedAt { get; set; }

    /// <summary>Ngày tạo order (để check SLA 24h)</summary>
    public DateTime OrderCreatedAt { get; set; }

    /// <summary>Số lượng items trong order</summary>
    public int ItemCount { get; set; } = 0;

    /// <summary>ID phiên bản sản phẩm chính</summary>
    public Guid? ProductVersionId { get; set; }

    /// <summary>Tên phiên bản sản phẩm chính</summary>
    public string? ProductVersionName { get; set; }

    /// <summary>ID category của sản phẩm chính</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Tên category của sản phẩm chính</summary>
    public string? CategoryName { get; set; }
}

/// <summary>
/// Event khi Order đang chờ lấy hàng (AwaitingPickup status)
/// OrderService publish → ShopService consume
/// Exchange: "order.events" / Routing key: "order.awaiting.pickup"
/// Sử dụng để:
/// - Update TaskBoard (AwaitingPickupCount)
/// - Track SLA violations (> 45 phút)
/// </summary>
public class OrderAwaitingPickupEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime AwaitingPickupAt { get; set; }
}

/// <summary>
/// Event khi Order đã xử lý, sẵn sàng giao vận chuyển (ReadyToShip status)
/// OrderService publish → ShopService consume
/// Exchange: "order.events" / Routing key: "order.ready.to.ship"
/// Sử dụng để:
/// - Update TaskBoard (ReadyToShipCount)
/// </summary>
public class OrderReadyToShipEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public DateTime ReadyToShipAt { get; set; }
}

/// <summary>
/// Batch event - Dùng khi cần push toàn bộ metrics cùng lúc
/// OrderService publish → ShopService consume
/// (Optional - dùng cho hot reload metrics khi restart)
/// </summary>
public class MetricsAggregatedEvent
{
    /// <summary>ID của shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>Số lượng orders chờ lấy hàng</summary>
    public int AwaitingPickupCount { get; set; }

    /// <summary>Số lượng orders ready to ship</summary>
    public int ReadyToShipCount { get; set; }

    /// <summary>Số lượng orders chờ xử lý trả/hoàn</summary>
    public int ReturnsRefundsCount { get; set; }

    /// <summary>Doanh thu hôm nay</summary>
    public long RevenueTodayCents { get; set; }

    /// <summary>Thời điểm aggregate</summary>
    public DateTime AggregatedAt { get; set; }
}
