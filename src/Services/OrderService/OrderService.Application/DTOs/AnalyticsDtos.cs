namespace OrderService.Application.DTOs;

/// <summary>
/// DTO cho Top Category data
/// Chứa thông tin category và số lượng sản phẩm đã bán
/// </summary>
public class TopCategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    
    /// <summary>
    /// Số lượng items đã bán từ category này
    /// </summary>
    public int TotalItemsSold { get; set; }
    
    /// <summary>
    /// Tổng doanh số (VND)
    /// </summary>
    public double TotalSalesRevenue { get; set; }
    
    /// <summary>
    /// Số sản phẩm published trong category
    /// </summary>
    public int PublishedProductCount { get; set; }
}

/// <summary>
/// DTO cho Top Product data
/// Chứa thông tin sản phẩm và metrics bán hàng
/// </summary>
public class TopProductDto
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Số lượng items đã bán
    /// </summary>
    public int TotalItemsSold { get; set; }
    
    /// <summary>
    /// Tổng doanh số (VND)
    /// </summary>
    public double TotalSalesRevenue { get; set; }
    
    /// <summary>
    /// Giá bán trung bình (VND)
    /// </summary>
    public double AveragePrice { get; set; }
    
    /// <summary>
    /// Thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }
}
