namespace Shared.Events;

/// <summary>
/// Event khi Shop được tạo mới
/// ShopService publish → AccountService consume, OrderService consume, ShipmentService consume
/// </summary>
public class ShopCreatedEvent
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public string? DefaultProviderServiceCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountCreatedEvent
{
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Event khi Shop info được cập nhật
/// ShopService publish → OrderService consume, ShipmentService consume
/// Exchange: "shop.events" / Routing key: "shop.updated"
/// </summary>
public class ShopUpdatedEvent
{
    public Guid ShopId { get; set; }
    /// <summary>Chủ shop — map vào owner_account_id (cùng ý nghĩa với ShopCreatedEvent.OwnerId).</summary>
    public Guid OwnerAccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? ShopPhone { get; set; }
    public string? ShopEmail { get; set; }
    public string? ShopAddress { get; set; }
    public string? ShopCity { get; set; }
    public string? ShopDistrict { get; set; }
    public string? ShopWard { get; set; }
    public string? ShopProvince { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public string? DefaultProviderServiceCode { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Shop bị xóa (mirror) — consumers xóa bản ghi shop local. Exchange shop.events, routing shop.deleted.
/// </summary>
public class ShopDeletedEvent
{
    public Guid ShopId { get; set; }
    public DateTime DeletedAt { get; set; }
}

/// <summary>
/// ProductService (hoặc service khác) publish — ShopService đọc DB và trả lời bằng <see cref="ShopNamesPublishedEvent"/>.
/// Exchange: shop.events / Routing key: shop.names.request
/// </summary>
public class ShopNamesRequestEvent
{
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}

/// <summary>
/// Danh sách shop id + tên (đồng bộ cache ProductService, không HTTP).
/// Exchange: shop.events / Routing key: shop.names.published
/// </summary>
public class ShopNamesPublishedEvent
{
    public DateTime PublishedAt { get; set; }
    public List<ShopNameRegistryEntry> Shops { get; set; } = new();
}

public class ShopNameRegistryEntry
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
}

/// <summary>
/// Event khi Order được tạo mới
/// OrderService publish → ShipmentService consume
/// Exchange: "order.events" / Routing key: "order.created"
/// </summary>
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string? DeliveryAddress { get; set; }
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public string? ProviderServiceCode { get; set; }
    public Guid? VoucherId { get; set; }

    /// <summary>Tổng cân nặng của tất cả items trong order (grams)</summary>
    public int TotalWeightGrams { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Event khi Shipping Fee được tính toán xong
/// ShipmentService publish → OrderService consume
/// Exchange: "shipment.events" / Routing key: "shippingfee.calculated"
/// </summary>
public class ShippingFeeCalculatedEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public long ShippingFeeVnd { get; set; }
    public string? ProviderServiceCode { get; set; }
    public bool IsFreeShipping { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// ShipmentService publishes when shipment lifecycle status changes.
/// OrderService consumes to sync <c>orders.status</c> and push SignalR (no cross-service HTTP).
/// Exchange: "shipment.events" / Routing key: "shipment.status.changed"
/// </summary>
public class ShipmentStatusChangedEvent
{
    public Guid ShipmentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }

    /// <summary>Buyer — from order cache in ShipmentService when available.</summary>
    public Guid? AccountId { get; set; }

    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

/// <summary>
/// One line in an order that may receive a product review after delivery.
/// </summary>
public class OrderReviewEligibleLineItem
{
    public Guid OrderItemId { get; set; }
    public Guid VersionId { get; set; }
}

/// <summary>
/// OrderService publishes when status becomes DELIVERED or COMPLETED so ProductService
/// can record purchase eligibility without HTTP calls (microservice-friendly).
/// Exchange: order.events / Routing key: order.review_eligible
/// </summary>
public class OrderReviewEligibleEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public List<OrderReviewEligibleLineItem> Lines { get; set; } = new();
    public DateTime EligibleAt { get; set; }
}

/// <summary>
/// Line for stock deduction when order is delivered (quantity sold from this version).
/// </summary>
public class OrderDeliveredStockLineItem
{
    public Guid VersionId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// OrderService publishes when order becomes <b>DELIVERED</b> — ProductService decrements <c>product_versions.stock_quantity</c> (idempotent per OrderId).
/// Not published for COMPLETED-from-payment alone.
/// Exchange: order.events / Routing key: order.delivered.stock
/// </summary>
public class OrderDeliveredStockEvent
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public List<OrderDeliveredStockLineItem> Lines { get; set; } = new();
    public DateTime DeliveredAt { get; set; }
}

/// <summary>
/// PaymentService publishes after gateway payment succeeds for one or more shop orders (e.g. PayOS PAID).
/// OrderService sets orders to COMPLETED (PayOS, wallet, etc.).
/// Exchange: payment.events / Routing key: payment.orders.paid
/// </summary>
public class PaymentOrdersPaidEvent
{
    public Guid PaymentId { get; set; }
    public Guid? AccountId { get; set; }
    public List<Guid> OrderIds { get; set; } = new();
    public string Provider { get; set; } = "PAYOS";
    public long ProviderOrderCode { get; set; }
    public long AmountVnd { get; set; }
    public DateTime PaidAt { get; set; }
}

/// <summary>
/// ProductService publishes after recomputing shop-level review aggregates from product reviews.
/// ShopService updates <c>shops.rating</c> and <c>shops.review_count</c> (no HTTP).
/// Exchange: shop.events / Routing key: shop.review_stats.updated
/// </summary>
public class ShopReviewStatsUpdatedEvent
{
    public Guid ShopId { get; set; }
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
