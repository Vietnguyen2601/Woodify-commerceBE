namespace ShipmentService.Domain.Entities;

/// <summary>
/// Mirror shop từ ShopService — bảng shop_cache (theo spec Order/Shipment).
/// </summary>
public class ShopCache
{
    public Guid ShopId { get; set; }
    public Guid OwnerAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
}
