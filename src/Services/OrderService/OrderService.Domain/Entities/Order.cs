namespace OrderService.Domain.Entities;

/// <summary>
/// Entity Order - Bảng Orders
/// Quản lý đơn hàng
/// </summary>
public class Order
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    
    // Các thông tin cần được làm rõ để đề phòng có sự thay đổi
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    
    // Tiền gốc - tiền sau các phí thêm và giảm giá
    public double SubtotalCents { get; set; }
    public double TotalAmountCents { get; set; }
    
    public Guid? VoucherId { get; set; }
    
    // Payment reference
    public Guid? Payment { get; set; }
    
    // Trạng thái - vận chuyển
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    
    // Cập nhập lại địa chỉ do đã bỏ bảng address
    public string? DeliveryAddressId { get; set; }
    
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
