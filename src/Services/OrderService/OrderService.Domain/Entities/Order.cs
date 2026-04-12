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
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; } // = SubtotalVnd + ShippingFee + Voucher

    public Guid? VoucherId { get; set; }

    // === THÊM MỚI ===
    // Hoa hồng
    public decimal CommissionRate { get; set; } = 0.06m;  // Tỷ lệ hoa hồng của đơn này
    public long CommissionVnd { get; set; } = 0;        // Số tiền hoa hồng đã tính

    // Trạng thái - vận chuyển
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;

    // Địa chỉ giao hàng (string, không phải ID)
    public string? DeliveryAddress { get; set; }

    /// <summary>Shipping service code at checkout (snapshot, max 20 chars).</summary>
    public string ProviderServiceCode { get; set; } = "STD";

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
