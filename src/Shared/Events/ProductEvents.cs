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
    public int? VersionNumber { get; set; }
    public string? VersionName { get; set; }

    // Pricing
    public double Price { get; set; }
    public string Currency { get; set; } = "VND";

    // Stock
    public int StockQuantity { get; set; } = 0;

    // Shipping Dimensions
    public string? WoodType { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Thumbnail
    public string? ThumbnailUrl { get; set; }

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

/// <summary>
/// Event được publish khi ProductMaster bị xóa (soft delete)
/// </summary>
public class ProductDeletedEvent
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
    public string EventType { get; set; } = "ProductDeleted";
}

/// <summary>
/// Event được publish khi ảnh của ProductVersion được thêm, cập nhật, hoặc xóa
/// </summary>
public class ImageUrlUpdatedEvent
{
    public Guid VersionId { get; set; }
    public string? ThumbnailUrl { get; set; } // URL ảnh chính (sort_order = 0)
    public DateTime UpdatedAt { get; set; }
    public string EventType { get; set; } = "ImageUrlUpdated"; // Updated, Deleted
}
