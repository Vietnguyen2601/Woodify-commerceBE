namespace ShopService.Repositories.Models;

/// <summary>
/// Entity Shop - Bảng Shops
/// Quản lý thông tin cửa hàng
/// </summary>
public class Shop
{
    public Guid ShopId { get; set; } = Guid.NewGuid();
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// ID của chủ shop (từ AccountService)
    /// </summary>
    public Guid OwnerId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
