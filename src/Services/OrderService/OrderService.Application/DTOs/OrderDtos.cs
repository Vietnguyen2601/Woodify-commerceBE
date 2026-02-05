namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để tạo order từ cart
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ShippingAddress { get; set; }
}

/// <summary>
/// DTO trả về thông tin order
/// </summary>
public class OrderDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    
    public string Currency { get; set; } = "VND";
    public long SubtotalCents { get; set; }
    public long ShippingFeeCents { get; set; }
    public long TotalAmountCents { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime PlacedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ShippingAddress { get; set; }
    
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
    
    public Guid? ProductId { get; set; }
    public Guid? ProductVersionId { get; set; }
    public string? SkuCode { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public long UnitPriceCents { get; set; }
    public int Qty { get; set; }
    public long TaxCents { get; set; }
    public long LineTotalCents { get; set; }
    
    public string FulfillmentStatus { get; set; } = string.Empty;
    public int ReturnedQty { get; set; }
    
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
    public Guid ProductVersionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}
