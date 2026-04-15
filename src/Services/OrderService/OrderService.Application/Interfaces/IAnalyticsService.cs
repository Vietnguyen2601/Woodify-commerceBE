using OrderService.Application.DTOs;
using Shared.Results;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface cho analytics service
/// Handling Top Categories, Top Products by Category, etc.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Lấy top N categories có sản phẩm bán chạy nhất
    /// Sắp xếp theo số lượng bán (từ OrderItems)
    /// </summary>
    /// <param name="topN">Số lượng categories muốn lấy (default: 10)</param>
    /// <returns>Top categories with sales count</returns>
    Task<ServiceResult<IEnumerable<TopCategoryDto>>> GetTopSellCategoriesAsync(int topN = 10);

    /// <summary>
    /// Lấy top N products bán chạy nhất trong một category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="topN">Số lượng products muốn lấy (default: 10)</param>
    /// <returns>Top products in category with sales info</returns>
    Task<ServiceResult<IEnumerable<TopProductDto>>> GetTopProductsByCategoryAsync(Guid categoryId, int topN = 10);

    /// <summary>
    /// Lấy top N products bán chạy nhất (tất cả categories)
    /// </summary>
    /// <param name="topN">Số lượng products muốn lấy (default: 20)</param>
    /// <returns>Top products overall with sales info</returns>
    Task<ServiceResult<IEnumerable<TopProductDto>>> GetTopProductsOverallAsync(int topN = 20);
}
