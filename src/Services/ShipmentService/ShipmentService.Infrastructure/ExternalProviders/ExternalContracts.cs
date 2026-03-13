namespace ShipmentService.Infrastructure.ExternalProviders;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public class OrderItemInfo
{
    public Guid VersionId { get; set; }
    public int Quantity { get; set; }
}

public class OrderContext
{
    public Guid ShopId { get; set; }
    public string? DeliveryAddressId { get; set; }
    public List<OrderItemInfo> Items { get; set; } = new();
}

public class ProductVersionInfo
{
    public Guid VersionId { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
}

// ── Interfaces ────────────────────────────────────────────────────────────────

public interface IOrderServiceClient
{
    Task<List<OrderItemInfo>?> GetOrderItemsAsync(Guid orderId);
    Task<OrderContext?> GetOrderContextAsync(Guid orderId);
}

public interface IProductServiceClient
{
    Task<ProductVersionInfo?> GetProductVersionAsync(Guid versionId);
}

public interface IShopServiceClient
{
    Task<string?> GetDefaultPickupAddressAsync(Guid shopId);
}
