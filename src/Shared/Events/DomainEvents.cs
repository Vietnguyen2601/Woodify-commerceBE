namespace Shared.Events;

/// <summary>
/// Event khi Shop được tạo mới
/// ShopService publish → AccountService consume
/// </summary>
public class ShopCreatedEvent
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
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
/// ShopService publish → ShipmentService consume
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
    public string? DeliveryAddressId { get; set; }
    public double TotalAmountCents { get; set; }
    public DateTime CreatedAt { get; set; }
}
