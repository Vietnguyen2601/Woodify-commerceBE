namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để tạo order từ cart
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public Guid? DeliveryAddressId { get; set; }
    public Guid? VoucherId { get; set; }
    public Guid? Payment { get; set; }
}

/// <summary>
/// DTO trả về thông tin order
/// </summary>
public class OrderDto
{
    public Guid OrderId { get; set; }
    
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    
    public double SubtotalCents { get; set; }
    public double TotalAmountCents { get; set; }
    
    public Guid? VoucherId { get; set; }
    
    public Guid? Payment { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public string? DeliveryAddressId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}

/// <summary>
/// DTO trả về thông tin order item
/// </summary>
public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VersionId { get; set; }
    
    public long UnitPriceCents { get; set; }
    public int Quantity { get; set; }
    public long DiscountCents { get; set; }
    public double TaxCents { get; set; }
    public double LineTotalCents { get; set; }
    
    public Guid? ShipmentId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO trả về kết quả validation cho checkout
/// </summary>
public class CheckoutValidationDto
{
    public bool IsValid { get; set; }
    public List<InvalidCartItemDto> InvalidItems { get; set; } = new List<InvalidCartItemDto>();
    public int TotalItems { get; set; }
    public int ValidItemsCount { get; set; }
    public int InvalidItemsCount { get; set; }
}

/// <summary>
/// DTO cho cart item không hợp lệ
/// </summary>
public class InvalidCartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid VersionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}
