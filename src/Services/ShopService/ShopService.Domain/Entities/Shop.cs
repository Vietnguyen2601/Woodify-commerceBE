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
    public decimal? Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ShopStatus Status { get; set; } = ShopStatus.ACTIVE;
    public int FollowerCount { get; set; } = 0;

    // Navigation property
    public virtual ICollection<ShopFollower> Followers { get; set; } = new List<ShopFollower>();
}
