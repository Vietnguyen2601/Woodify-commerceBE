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
    public double SubtotalCents { get; set; }
    public double TotalAmountCents { get; set; }
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
    public long ShippingFeeCents { get; set; }
    public string? ProviderServiceCode { get; set; }
    public bool IsFreeShipping { get; set; }
    public DateTime CalculatedAt { get; set; }
}
