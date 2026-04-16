namespace OrderService.Application.DTOs;

/// <summary>
/// Query DTO cho admin GetAllOrders (pagination + filter)
/// </summary>
public class GetAllOrdersQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>L?c theo tr?ng th?i: PENDING, CONFIRMED, PROCESSING, SHIPPED, DELIVERED, COMPLETED, CANCELLED...</summary>
    public string? Status { get; set; }

    /// <summary>L?c theo shop</summary>
    public Guid? ShopId { get; set; }

    /// <summary>L?c theo account (customer)</summary>
    public Guid? AccountId { get; set; }
}

/// <summary>
/// K?t qu? tr? v? t? GetAllOrders (paginated)
/// </summary>
public class OrderListResultDto
{
    public List<OrderDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// Request t?o Order t? m?t shop (v2 refactored)
/// 
/// Workflow: Frontend group items by shop, sau d? g?i API n?y N l?n (1 l?n per shop)
/// V? d?: User ch?n t? 2 shops ? Frontend g?i CreateOrder 2 l?n
/// </summary>
public class CreateOrderRequest
{
    /// <summary>ID t?i kho?n customer</summary>
    public Guid AccountId { get; set; }

    /// <summary>ID shop (required - kh?ng th? null v? m?i call ch? process 1 shop)</summary>
    public Guid ShopId { get; set; }

    /// <summary>IDs c?a cart items user ch?n t? shop n?y (required - ph?i c? ?t nh?t 1 item)</summary>
    public Guid[] CartItemIds { get; set; } = Array.Empty<Guid>();

    /// <summary>??a ch? giao h?ng (required)</summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>M? phuong th?c v?n chuy?n EXP, STD, ECO (FAST); legacy EXPRESS/STANDARD/ECONOMY normalized on save</summary>
    public string ProviderServiceCode { get; set; } = "STD";

    /// <summary>ID voucher (optional)</summary>
    public Guid? VoucherId { get; set; }
}

/// <summary>Pre-order shipping preview for one shop ? same cart lines as <see cref="CreateOrderRequest"/> (no delivery address).</summary>
public class CheckoutShippingPreviewRequest
{
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public Guid[] CartItemIds { get; set; } = Array.Empty<Guid>();
}

public class CheckoutShippingPreviewOptionDto
{
    public string ProviderServiceCode { get; set; } = string.Empty;
    public string DisplayLabel { get; set; } = string.Empty;
    /// <summary>Freight only (VND) ? FE d?ng field n?y ?? hi?n th? ph? ship. Tr?ng <see cref="CreateOrderResponse.ShippingFeeVnd"/> khi ch?n tier n?y.</summary>
    public double TotalAmountVnd { get; set; }
    public bool IsFreeShipping { get; set; }
}

public class CheckoutShippingPreviewResponseDto
{
    public Guid ShopId { get; set; }
    public double SubtotalVnd { get; set; }
    public int TotalWeightGrams { get; set; }
    public long FreeShippingThresholdVnd { get; set; }
    public bool SubtotalQualifiesForFreeShipping { get; set; }
    public List<CheckoutShippingPreviewOptionDto> Options { get; set; } = new();
}

/// <summary>
/// Legacy DTO - gi? l?i cho backward compatibility (n?u c?n)
/// </summary>
public class CreateOrderFromCartDto
{
    public Guid AccountId { get; set; }
    public Guid ShopId { get; set; }
    public string? DeliveryAddress { get; set; }
    public Guid? VoucherId { get; set; }
    /// <summary>M? d?ch v? VC (max 20). M?c d?nh STD.</summary>
    public string ProviderServiceCode { get; set; } = "STD";
    public Guid[]? SelectedCartItemIds { get; set; }
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Legacy Response DTO - gi? l?i cho backward compatibility (n?u c?n)
/// </summary>
public class CreateOrdersFromCartResultDto
{
    public List<Guid> OrderIds { get; set; } = new List<Guid>();
    public long TotalAmountVnd { get; set; }
    public int OrderCount { get; set; }
    public List<OrderSummaryDto> Orders { get; set; } = new List<OrderSummaryDto>();
}

/// <summary>
/// Response t?o Order - tr? v? 1 order object (v2 refactored)
/// 
/// Frontend s?:
/// 1. G?i CreateOrder cho m?i shop ? nh?n orderId + totalAmount
/// 2. Sum t?t c? totalAmount ? g?i CreatePayment 1 l?n
/// </summary>
public class CreateOrderResponse
{
    /// <summary>ID order v?a t?o</summary>
    public Guid OrderId { get; set; }

    /// <summary>ID shop</summary>
    public Guid ShopId { get; set; }

    /// <summary>S? ti?n s?n ph?m (kh?ng bao g?m shipping fee)</summary>
    public double SubtotalVnd { get; set; }

    /// <summary>Ph? v?n chuy?n (cents)</summary>
    public long ShippingFeeVnd { get; set; }

    /// <summary>S? ti?n hoa h?ng 6% (cents)</summary>
    public long CommissionVnd { get; set; }

    /// <summary>T?ng ti?n = SubtotalVnd + ShippingFeeVnd (bao g?m m?i chi ph?)</summary>
    public double TotalAmountVnd { get; set; }

    /// <summary>S? lu?ng items trong order n?y</summary>
    public int ItemCount { get; set; }

    /// <summary>Tr?ng th?i order (m?c d?nh: PENDING - ch? thanh to?n)</summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>Th?i di?m t?o</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>M? d?ch v? v?n chuy?n d? luu tr?n don</summary>
    public string ProviderServiceCode { get; set; } = "STD";
}

/// <summary>
/// Legacy DTO - gi? l?i cho backward compatibility (n?u c?n)
/// </summary>
public class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public long CommissionVnd { get; set; }
    public int ItemCount { get; set; }
    public string ProviderServiceCode { get; set; } = "STD";
}

/// <summary>
/// DTO tr? v? th?ng tin order
/// </summary>
public class OrderDto
{
    public Guid OrderId { get; set; }

    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    /// <summary>Customer email mirrored from Identity via account_directory.</summary>
    public string? AccountEmail { get; set; }
    public Guid ShopId { get; set; }

    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }

    /// <summary>
    /// === TI?N H?A H?NG ===
    /// T? l? hoa h?ng s?n l?y t? don h?ng n?y (m?c d?nh 6%)
    /// </summary>
    public decimal CommissionRate { get; set; } = 0.06m;

    /// <summary>
    /// S? ti?n hoa h?ng d? t?nh (cents)
    /// Formula: FLOOR(subtotal_cents ? commission_rate)
    /// Example: 1,000,000 ? 0.06 = 60,000 cents
    /// D?ng sau d? tr? t? Seller Wallet khi Order ? COMPLETED
    /// </summary>
    public long CommissionVnd { get; set; } = 0;

    public Guid? VoucherId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? DeliveryAddress { get; set; }

    /// <summary>M? d?ch v? v?n chuy?n (snapshot tr?n don)</summary>
    public string ProviderServiceCode { get; set; } = "STD";

    /// <summary>
    /// Payment URL t? PayOS d? thanh to?n tr?c tuy?n
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// QR Code URL t? PayOS
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Tr?ng th?i payment (PENDING, PAID, EXPIRED, CANCELLED)
    /// </summary>
    public string? PaymentStatus { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}

/// <summary>
/// DTO tr? v? th?ng tin order item
/// </summary>
public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VersionId { get; set; }

    public long UnitPriceVnd { get; set; }
    public int Quantity { get; set; }
    public long DiscountVnd { get; set; }
    public double TaxVnd { get; set; }
    public double LineTotalVnd { get; set; }

    public Guid? ShipmentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO tr? v? k?t qu? validation cho checkout
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
/// DTO cho cart item kh?ng h?p l?
/// </summary>
public class InvalidCartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid VersionId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}

/// <summary>
/// DTO tr? v? order v?i chi ti?t s?n ph?m cho seller
/// </summary>
public class OrderWithProductDetailsDto
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public Guid ShopId { get; set; }
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public Guid? VoucherId { get; set; }
    public Guid? Payment { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public string ProviderServiceCode { get; set; } = "STD";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemWithProductDetailsDto> OrderItems { get; set; } = new List<OrderItemWithProductDetailsDto>();
}

/// <summary>
/// DTO tr? v? order item k?m chi ti?t s?n ph?m
/// </summary>
public class OrderItemWithProductDetailsDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VersionId { get; set; }
    public long UnitPriceVnd { get; set; }
    public int Quantity { get; set; }
    public long DiscountVnd { get; set; }
    public double TaxVnd { get; set; }
    public double LineTotalVnd { get; set; }
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
/// Order + line items + product cache for customer (GET Account?accountId). No ShipmentId on line items.
/// </summary>
public class CustomerAccountOrderDto
{
    public Guid OrderId { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public Guid ShopId { get; set; }
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public Guid? VoucherId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public string ProviderServiceCode { get; set; } = "STD";
    /// <summary>Payment status for UI (derived from Order.Status; PaymentService not integrated).</summary>
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CustomerAccountOrderItemDto> OrderItems { get; set; } = new();
}

/// <summary>
/// Line item with product snapshot (same cache logic as Shop). No ShipmentId.
/// </summary>
public class CustomerAccountOrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid VersionId { get; set; }
    public long UnitPriceVnd { get; set; }
    public int Quantity { get; set; }
    public long DiscountVnd { get; set; }
    public double TaxVnd { get; set; }
    public double LineTotalVnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

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
/// Top-selling products (ProductMaster scope): ranks by units sold; display fields from Order DB product_version_cache + shop_info_cache.
/// </summary>
public class TopSellingProductAnalyticsDto
{
    public int Rank { get; set; }
    public Guid ProductId { get; set; }
    public long UnitsSold { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string? ProductStatus { get; set; }
    public string? ThumbnailUrl { get; set; }

    /// <summary>Representative version row used for display (SKU / deep link to Product service).</summary>
    public Guid? VersionId { get; set; }
    public string? SellerSku { get; set; }

    public Guid ShopId { get; set; }
    public string? ShopName { get; set; }
}

/// <summary>
/// DTO d? c?p nh?t tr?ng th?i order
/// </summary>
public class UpdateOrderStatusDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}
