namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để tạo order từ cart
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public Guid? DeliveryAddressId { get; set; }
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
    public string? CustomerNote { get; set; }
}

/// <summary>
/// DTO trả về thông tin order
/// </summary>
public class OrderDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    
    public Guid? AccountId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    
    public Guid? ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    
    public string Currency { get; set; } = "VND";
    public long SubtotalCents { get; set; }
    public long ShippingFeeCents { get; set; }
    public long DiscountCents { get; set; }
    public long TaxCents { get; set; }
    public long TotalAmountCents { get; set; }
    
    public Guid? VoucherApplied { get; set; }
    
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentTransactionId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public Guid? DeliveryAddressId { get; set; }
    
    public string? CustomerNote { get; set; }
    public string? ShopNote { get; set; }
    
    public DateTime PlacedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public string? CancelReason { get; set; }
    public Guid? CancelledBy { get; set; }
    
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
    
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string SellerSku { get; set; } = string.Empty;
    
    public long UnitPriceCents { get; set; }
    public int Quantity { get; set; }
    public long DiscountCents { get; set; }
    public long TaxCents { get; set; }
    public long LineTotalCents { get; set; }
    
    public Guid? ShipmentId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public int ReturnedQuantity { get; set; }
    public long RefundedAmountCents { get; set; }
    
    public string? ShippingInfo { get; set; }
    
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
