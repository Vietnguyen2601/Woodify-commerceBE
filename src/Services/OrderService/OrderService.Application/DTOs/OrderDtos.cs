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
/// DTO để tạo order từ cart - hỗ trợ checkout selected items
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public string? DeliveryAddress { get; set; }
    public Guid? VoucherId { get; set; }

    /// <summary>
    /// Mã phương thức vận chuyển mà customer chọn (ví dụ: "EXPRESS", "STANDARD")
    /// </summary>
    public string? ProviderServiceCode { get; set; }

    /// <summary>
    /// IDs của cart items cần thanh toán. Nếu null/empty → thanh toán toàn bộ cart (backward compatible)
    /// </summary>
    public Guid[]? SelectedCartItemIds { get; set; }

    /// <summary>
    /// Phương thức thanh toán (COD, WALLET, PAYOS) - chỉ dùng để log, CreateOrdersFromCart không xử lý payment
    /// Payment sẽ được xử lý riêng ở PaymentService sau bước này
    /// </summary>
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// DTO trả về kết quả tạo orders từ cart - danh sách orderIds
/// </summary>
public class CreateOrdersFromCartResultDto
{
    /// <summary>
    /// Danh sách IDs của orders vừa tạo (mỗi shop 1 order)
    /// </summary>
    public List<Guid> OrderIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Tổng số tiền cần thanh toán (sum của tất cả orders' TotalAmountCents)
    /// </summary>
    public long TotalAmountCents { get; set; }

    /// <summary>
    /// Số lượng orders được tạo
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Chi tiết từng order (optional - dùng để client preview)
    /// </summary>
    public List<OrderSummaryDto> Orders { get; set; } = new List<OrderSummaryDto>();
}

/// <summary>
/// DTO tóm tắt thông tin order (dùng trong CreateOrdersFromCartResultDto)
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
