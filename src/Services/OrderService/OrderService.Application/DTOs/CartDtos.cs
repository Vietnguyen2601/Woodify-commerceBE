namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để thêm sản phẩm vào giỏ hàng
/// </summary>
public class AddToCartDto
{
    public Guid VersionId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// DTO để cập nhật số lượng sản phẩm trong giỏ
/// </summary>
public class UpdateCartItemDto
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// DTO trả về thông tin giỏ hàng
/// </summary>
public class CartDto
{
    public Guid CartId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    public double TotalPrice { get; set; }
    public int TotalItems { get; set; }
}

/// <summary>
/// DTO trả về thông tin item trong giỏ hàng
/// </summary>
public class CartItemDto
{
    public Guid CartItemId { get; set; }
    public Guid CartId { get; set; }
    public Guid VersionId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public double TotalPrice { get; set; }
    public string? ProductMasterName { get; set; }
    public string? ProductVersionName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? ShopName { get; set; }  // Shop name từ ShopCache
}

/// <summary>
/// DTO cho Checkout Preview - hiển thị giỏ hàng trước khi tạo order
/// </summary>
public class CheckoutPreviewDto
{
    public Guid CartId { get; set; }
    public Guid AccountId { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
    public double Subtotal { get; set; }
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
    public Guid VersionId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
    public double TotalPrice { get; set; }
    public bool IsValid { get; set; } = true;
    public string? InvalidReason { get; set; }
}
