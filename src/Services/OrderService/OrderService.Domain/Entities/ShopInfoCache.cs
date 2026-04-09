namespace OrderService.Domain.Entities;

/// <summary>
/// Mirror shop từ ShopService (RabbitMQ shop.events) — đọc local cho cart/checkout.
/// </summary>
public class ShopInfoCache
{
    public Guid ShopId { get; set; }
    public Guid OwnerAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public string? DefaultProviderServiceCode { get; set; }
    public DateTime UpdatedAt { get; set; }
}
