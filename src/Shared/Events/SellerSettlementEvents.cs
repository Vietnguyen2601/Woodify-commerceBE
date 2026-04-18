namespace Shared.Events;

/// <summary>
/// Đơn đã thanh toán xong (COMPLETED) — ghi có net vào ví seller (chủ shop).
/// Exchange: order.events / Routing key: order.seller.net.eligible
/// </summary>
public class OrderSellerNetEligibleEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }

    /// <summary>Chủ shop — ghi có vào ví chính của account (cùng ví tạo khi đăng ký).</summary>
    public Guid SellerAccountId { get; set; }

    public long TotalAmountVnd { get; set; }
    public long CommissionVnd { get; set; }
    public long NetAmountVnd { get; set; }

    public DateTime OccurredAt { get; set; }

    /// <summary>Idempotent key cố định: order_net:{OrderId}</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>
/// Đơn từ COMPLETED chuyển sang CANCELLED/REFUNDED — hoàn tác ghi có net (ghi nợ seller).
/// Exchange: order.events / Routing key: order.seller.net.reversed
/// </summary>
public class OrderSellerNetReversedEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid SellerAccountId { get; set; }

    public long NetAmountVnd { get; set; }
    public DateTime OccurredAt { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;
}
