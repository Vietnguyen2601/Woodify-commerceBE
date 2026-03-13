using ShopService.Domain.Enums;

namespace ShopService.Domain.Entities;

/// <summary>
/// Entity Shop - Bảng Shops
/// </summary>
public class Shop
{
    public Guid ShopId { get; set; } = Guid.NewGuid();
    public Guid OwnerAccountId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }

    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }

    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public int TotalProducts { get; set; } = 0;
    public int TotalOrders { get; set; } = 0;

    public ShopStatus Status { get; set; } = ShopStatus.INACTIVE;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<ShopFollower> Followers { get; set; } = new List<ShopFollower>();
}
