using ProductService.Domain.Entities;

namespace ProductService.Application.DTOs;

public class CreateProductMasterDto
{
    public Guid ShopId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImgUrl { get; set; }
    public bool ArAvailable { get; set; } = false;
    public string? ArModelUrl { get; set; }
}

public class UpdateProductMasterDto
{
    public Guid? CategoryId { get; set; }
    public string? Name { get; set; }
    public string? GlobalSku { get; set; }
    public string? ImgUrl { get; set; }
    public string? Description { get; set; }
    public bool? ArAvailable { get; set; }
    public string? ArModelUrl { get; set; }
    public ProductStatus? Status { get; set; }
    public decimal? AvgRating { get; set; }
    public int? ReviewCount { get; set; }
    public int? SoldCount { get; set; }
}

public class ModerateProductDto
{
    public ModerationStatus ModerationStatus { get; set; }
    public Guid ModeratedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? ModerationNotes { get; set; }
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
    /// Minimum average rating (0-5)
    /// </summary>
    public decimal? MinRating { get; set; }
    
    /// <summary>
    /// Maximum average rating (0-5)
    /// </summary>
    public decimal? MaxRating { get; set; }
    
    /// <summary>
    /// Filter by AR availability
    /// </summary>
    public bool? ArAvailable { get; set; }
    
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
    /// Options: CreatedAt, AvgRating, Name, SoldCount
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
    
    public string? ImgUrl { get; set; }
    public string? Description { get; set; }
    
    public bool ArAvailable { get; set; }
    public string? ArModelUrl { get; set; }
    
    public decimal AvgRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    
    public ProductStatus Status { get; set; }
    
    // Moderation info
    public ModerationStatus ModerationStatus { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? ModerationNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
