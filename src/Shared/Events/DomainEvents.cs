namespace Shared.Events;

/// <summary>
/// Event khi Shop được tạo mới
/// ShopService publish → AccountService consume
/// </summary>
public class ShopCreatedEvent
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class AccountCreatedEvent
{
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
