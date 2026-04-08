namespace OrderService.Application.DTOs;

/// <summary>
/// Query DTO cho admin GetAllOrders (pagination + filter)
/// </summary>
public class GetAllOrdersQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>Lọc theo trạng thái: PENDING, CONFIRMED, PROCESSING, SHIPPED, DELIVERED, COMPLETED, CANCELLED...</summary>
    public string? Status { get; set; }

    /// <summary>Lọc theo shop</summary>
    public Guid? ShopId { get; set; }

    /// <summary>Lọc theo account (customer)</summary>
    public Guid? AccountId { get; set; }
}

/// <summary>
/// Kết quả trả về từ GetAllOrders (paginated)
/// </summary>
public class OrderListResultDto
{
    public List<OrderDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// Request tạo Order từ một shop (v2 refactored)
/// 
/// Workflow: Frontend group items by shop, sau đó gọi API này N lần (1 lần per shop)
/// Ví dụ: User chọn từ 2 shops → Frontend gọi CreateOrder 2 lần
/// </summary>
public class CreateOrderRequest
{
    /// <summary>ID tài khoản customer</summary>
    public Guid AccountId { get; set; }

    /// <summary>ID shop (required - không thể null vì mỗi call chỉ process 1 shop)</summary>
    public Guid ShopId { get; set; }

    /// <summary>IDs của cart items user chọn từ shop này (required - phải có ít nhất 1 item)</summary>
    public Guid[] CartItemIds { get; set; } = Array.Empty<Guid>();

    /// <summary>Địa chỉ giao hàng (required)</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>Mã phương thức vận chuyển (ví dụ: "EXPRESS", "FAST", "STANDARD", "ECONOMY")</summary>
    public string ProviderServiceCode { get; set; } = "STANDARD";

    /// <summary>ID voucher (optional)</summary>
    public Guid? VoucherId { get; set; }
}

/// <summary>
/// Legacy DTO - giữ lại cho backward compatibility (nếu cần)
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public string? DeliveryAddress { get; set; }
    public Guid? VoucherId { get; set; }
    public string? ProviderServiceCode { get; set; }
    public Guid[]? SelectedCartItemIds { get; set; }
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Legacy Response DTO - giữ lại cho backward compatibility (nếu cần)
/// </summary>
public class CreateOrdersFromCartResultDto
{
    public List<Guid> OrderIds { get; set; } = new List<Guid>();
    public long TotalAmountCents { get; set; }
    public int OrderCount { get; set; }
    public List<OrderSummaryDto> Orders { get; set; } = new List<OrderSummaryDto>();
}

/// <summary>
/// Response tạo Order - trả về 1 order object (v2 refactored)
/// 
/// Frontend sẽ:
/// 1. Gọi CreateOrder cho mỗi shop → nhận orderId + totalAmount
/// 2. Sum tất cả totalAmount → gọi CreatePayment 1 lần
/// </summary>
public class CreateOrderResponse
{
    /// <summary>ID order vừa tạo</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>Số tiền sản phẩm (không bao gồm shipping fee)</summary>
    public double SubtotalCents { get; set; }

    /// <summary>Phí vận chuyển (cents)</summary>
    public long ShippingFeeCents { get; set; }

    /// <summary>Số tiền hoa hồng 6% (cents)</summary>
    public long CommissionCents { get; set; }

    /// <summary>Tổng tiền = SubtotalCents + ShippingFeeCents (bao gồm mọi chi phí)</summary>
    public double TotalAmountCents { get; set; }

    /// <summary>Số lượng items trong order này</summary>
    public int ItemCount { get; set; }

    /// <summary>Trạng thái order (mặc định: PENDING - chờ thanh toán)</summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>Thời điểm tạo</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Legacy DTO - giữ lại cho backward compatibility (nếu cần)
/// </summary>
public class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public double SubtotalCents { get; set; }
    public double TotalAmountCents { get; set; }
    public long CommissionCents { get; set; }
    public int ItemCount { get; set; }
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

    /// <summary>
    /// === TIỀN HÓA HỒNG ===
    /// Tỷ lệ hoa hồng sàn lấy từ đơn hàng này (mặc định 6%)
    /// </summary>
    public decimal CommissionRate { get; set; } = 0.06m;

    /// <summary>
    /// Số tiền hoa hồng đã tính (cents)
    /// Formula: FLOOR(subtotal_cents × commission_rate)
    /// Example: 1,000,000 × 0.06 = 60,000 cents
    /// Dùng sau để trừ từ Seller Wallet khi Order → COMPLETED
    /// </summary>
    public long CommissionCents { get; set; } = 0;

    public Guid? VoucherId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? DeliveryAddress { get; set; }

    /// <summary>
    /// Payment URL từ PayOS để thanh toán trực tuyến
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// QR Code URL từ PayOS
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Trạng thái payment (PENDING, PAID, EXPIRED, CANCELLED)
    /// </summary>
    public string? PaymentStatus { get; set; }

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

/// <summary>
/// DTO trả về order với chi tiết sản phẩm cho seller
/// </summary>
public class OrderWithProductDetailsDto
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public double SubtotalCents { get; set; }
    public double TotalAmountCents { get; set; }
    public Guid? VoucherId { get; set; }
    public Guid? Payment { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemWithProductDetailsDto> OrderItems { get; set; } = new List<OrderItemWithProductDetailsDto>();
}

/// <summary>
/// DTO trả về order item kèm chi tiết sản phẩm
/// </summary>
public class OrderItemWithProductDetailsDto
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

    // Product details
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string SellerSku { get; set; } = string.Empty;
    public string? VersionName { get; set; }
    public string? WoodType { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
}

/// <summary>
/// DTO để cập nhật trạng thái order
/// </summary>
public class UpdateOrderStatusDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
