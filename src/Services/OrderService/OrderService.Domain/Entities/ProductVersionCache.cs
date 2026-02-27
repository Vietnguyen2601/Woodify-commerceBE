using OrderService.Domain.Entities;

namespace OrderService.Domain.Entities;

/// <summary>
/// Cache local của Product Version data từ ProductService
/// Được đồng bộ thông qua RabbitMQ events
/// </summary>
public class ProductVersionCache
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    
    // Product Master Info
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string ProductStatus { get; set; } = "DRAFT"; // Status của ProductMaster
    
    // Version Info
    public string SellerSku { get; set; } = string.Empty;
    public string? VersionName { get; set; }
    
    // Pricing
    public double Price { get; set; }
    public string Currency { get; set; } = "VND";
    
    // Pricing in cents (computed from Price for precise calculations)
    public long PriceCents { get; set; }
    public long BasePriceCents { get; set; }
    
    // Stock
    public int StockQuantity { get; set; } = 0;
    public bool AllowBackorder { get; set; } = false;
    
    // Shipping Dimensions
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Sync tracking
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
