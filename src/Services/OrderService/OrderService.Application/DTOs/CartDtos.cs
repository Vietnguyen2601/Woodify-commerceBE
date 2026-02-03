namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để thêm sản phẩm vào giỏ hàng
/// </summary>
public class AddToCartDto
{
    public Guid ProductVersionId { get; set; }
    public int Qty { get; set; } = 1;
}

/// <summary>
/// DTO để cập nhật số lượng sản phẩm trong giỏ
/// </summary>
public class UpdateCartItemDto
{
    public Guid CartItemId { get; set; }
    public int Qty { get; set; }
}

/// <summary>
/// DTO trả về thông tin giỏ hàng
/// </summary>
public class CartDto
{
    public Guid CartId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public long TotalPriceCents { get; set; }
    public int TotalItems { get; set; }
}

/// <summary>
/// DTO trả về thông tin item trong giỏ hàng
/// </summary>
public class CartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductVersionId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long UnitPriceCents { get; set; }
    public int Qty { get; set; }
    public long TotalPriceCents { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO cho Checkout Preview - hiển thị giỏ hàng trước khi tạo order
/// </summary>
public class CheckoutPreviewDto
{
    public Guid CartId { get; set; }
    public Guid AccountId { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
    public long SubtotalCents { get; set; }
    public int TotalItems { get; set; }
    public int ValidItemsCount { get; set; }
    public int InvalidItemsCount { get; set; }
    public bool HasInvalidItems { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO cho từng item trong checkout preview
/// </summary>
public class CheckoutItemDto
{
    public Guid CartItemId { get; set; }
    public Guid ProductVersionId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public long UnitPriceCents { get; set; }
    public int Qty { get; set; }
    public long TotalPriceCents { get; set; }
    public bool IsValid { get; set; } = true;
    public string? InvalidReason { get; set; }
    public DateTime AddedAt { get; set; }
}
