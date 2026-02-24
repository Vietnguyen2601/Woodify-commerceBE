namespace OrderService.Domain.Entities;

/// <summary>
/// Entity Order - Bảng Orders
/// Quản lý đơn hàng
/// </summary>
public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public string OrderCode { get; set; } = string.Empty;
    
    // Các thông tin cần được làm rõ để đề phòng có sự thay đổi
    public Guid? AccountId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    
    public Guid? ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    
    public string Currency { get; set; } = "VND";
    public long SubtotalCents { get; set; }
    public long ShippingFeeCents { get; set; } = 0;
    public long DiscountCents { get; set; } = 0;
    public long TaxCents { get; set; } = 0;
    public long TotalAmountCents { get; set; }
    
    public Guid? VoucherApplied { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;
    public string? PaymentTransactionId { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    
    public Guid? DeliveryAddressId { get; set; }
    
    public string? CustomerNote { get; set; }
    public string? ShopNote { get; set; }
    
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public string? CancelReason { get; set; }
    public Guid? CancelledBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum OrderStatus
{
    PENDING,
    CONFIRMED,
    PROCESSING,
    READY_TO_SHIP,
    SHIPPED,
    DELIVERED,
    COMPLETED,
    CANCELLED,
    REFUNDING,
    REFUNDED
}

public enum PaymentMethod
{
    BANK_TRANSFER,
    VNPAY,
    WALLET
}

public enum PaymentStatus
{
    PENDING,
    PAID,
    FAILED,
    REFUNDED,
    PARTIALLY_REFUNDED
}
