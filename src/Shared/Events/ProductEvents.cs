namespace Shared.Events;

/// <summary>
/// Event được publish khi ProductVersion được tạo hoặc cập nhật
/// </summary>
public class ProductVersionUpdatedEvent
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    
    // Product Master Info
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string ProductStatus { get; set; } = "DRAFT";
    
    // Version Info
    public string SellerSku { get; set; } = string.Empty;
    public int VersionNumber { get; set; } = 1;
    public string? VersionName { get; set; }
    
    // Pricing
    public long PriceCents { get; set; }
    public long? BasePriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    
    // Stock
    public int StockQuantity { get; set; } = 0;
    public int LowStockThreshold { get; set; } = 5;
    public bool AllowBackorder { get; set; } = false;
    
    // Shipping Dimensions
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public long? VolumeCm3 { get; set; }
    
    // Shipping Properties
    public string? BulkyType { get; set; }
    public bool IsFragile { get; set; } = false;
    public bool RequiresSpecialHandling { get; set; } = false;
    
    // Warranty
    public int WarrantyMonths { get; set; } = 12;
    public string? WarrantyTerms { get; set; }
    
    // Bundle
    public bool IsBundle { get; set; } = false;
    public long BundleDiscountCents { get; set; } = 0;
    
    // Images
    public string? PrimaryImageUrl { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    
    public DateTime UpdatedAt { get; set; }
    public string EventType { get; set; } = "ProductVersionUpdated"; // Created, Updated, Deleted
}

/// <summary>
/// Event được publish khi ProductMaster status thay đổi
/// </summary>
public class ProductStatusChangedEvent
{
    public Guid ProductId { get; set; }
    public string Status { get; set; } = string.Empty; // DRAFT, PUBLISHED, ARCHIVED, DELETED
    public DateTime ChangedAt { get; set; }
    public string EventType { get; set; } = "ProductStatusChanged";
}

/// <summary>
/// Event được publish khi ProductVersion bị xóa
/// </summary>
public class ProductVersionDeletedEvent
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime DeletedAt { get; set; }
    public string EventType { get; set; } = "ProductVersionDeleted";
}

/// <summary>
/// Event được publish khi ProductVersion được khôi phục
/// </summary>
public class ProductVersionRestoredEvent
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime RestoredAt { get; set; }
    public string EventType { get; set; } = "ProductVersionRestored";
}
