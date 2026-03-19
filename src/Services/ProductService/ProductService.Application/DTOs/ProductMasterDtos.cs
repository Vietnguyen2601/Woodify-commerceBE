using ProductService.Domain.Entities;

namespace ProductService.Application.DTOs;

public class CreateProductMasterDto
{
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProductMasterDto
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class ModerateProductDto
{
    public ModerationStatus ModerationStatus { get; set; }
}

public class ProductSearchDto
{
    /// <summary>
    /// Search keyword for product name
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// Filter by product status (default: PUBLISHED only)
    /// </summary>
    public ProductStatus? Status { get; set; }

    /// <summary>
    /// Filter by category name
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Filter by moderation status
    /// </summary>
    public ModerationStatus? ModerationStatus { get; set; }

    /// <summary>
    /// Page number (starts from 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field (default: CreatedAt)
    /// Options: CreatedAt, Name, PublishedAt
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (default: DESC)
    /// </summary>
    public string SortDirection { get; set; } = "DESC";
}

public class ProductSearchResultDto
{
    public List<ProductMasterDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ProductMasterDto
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? GlobalSku { get; set; }

    public string? Description { get; set; }

    public ProductStatus Status { get; set; }

    // Moderation info
    public ModerationStatus ModerationStatus { get; set; }
    public DateTime? ModeratedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>Primary image URL (SortOrder = 0), null if no image uploaded</summary>
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// DTO for product detail with versions
/// Used for seller/admin (full info) and buyer (published only)
/// </summary>
public class ProductMasterDetailDto
{
    public Guid ProductId { get; set; }
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? GlobalSku { get; set; }
    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? ModerationStatus { get; set; }
    public DateTime? ModeratedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    /// <summary>All images for this product, sorted by SortOrder</summary>
    public List<ImageUrlDto> Images { get; set; } = new();

    public List<ProductVersionDetailDto> Versions { get; set; } = new();
}

/// <summary>
/// Simplified version DTO for product detail response
/// </summary>
public class ProductVersionDetailDto
{
    public Guid VersionId { get; set; }
    public int? VersionNumber { get; set; }
    public string? VersionName { get; set; }
    public double Price { get; set; }
    public int StockQuantity { get; set; }
    public string? WoodType { get; set; }
    public int WeightGrams { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>All images for this version, sorted by SortOrder</summary>
    public List<ImageUrlDto> Images { get; set; } = new();
}

/// <summary>
/// Paginated result for product details with versions
/// </summary>
public class ProductDetailListResultDto
{
    public List<ProductMasterDetailDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Filter DTO for Admin's Pending Approval Queue
/// Queue = status=PENDING_APPROVAL and moderation_status=PENDING
/// </summary>
public class PendingApprovalQueueFilterDto
{
    /// <summary>
    /// Filter by category ID (optional)
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Filter by shop ID (optional)
    /// </summary>
    public Guid? ShopId { get; set; }

    /// <summary>
    /// Filter by submission time from (optional)
    /// </summary>
    public DateTime? SubmittedFrom { get; set; }

    /// <summary>
    /// Filter by submission time to (optional)
    /// </summary>
    public DateTime? SubmittedTo { get; set; }

    /// <summary>
    /// Page number (starts from 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Result DTO for Admin's Pending Approval Queue
/// </summary>
public class PendingApprovalQueueResultDto
{
    public List<ProductMasterDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
