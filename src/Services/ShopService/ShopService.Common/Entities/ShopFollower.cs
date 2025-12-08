namespace ShopService.Common.Entities;

/// <summary>
/// Entity ShopFollower - Bảng Shop_Follower
/// </summary>
public class ShopFollower
{
    public Guid ShopFollowerId { get; set; } = Guid.NewGuid();
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Shop? Shop { get; set; }
}
