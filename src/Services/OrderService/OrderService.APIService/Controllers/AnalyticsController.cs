using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace OrderService.APIService.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Lấy top N categories có sản phẩm bán chạy nhất
    /// Sắp xếp theo số lượng items bán từ các sản phẩm trong category
    /// </summary>
    /// <remarks>
    /// GET /api/analytics/top-categories?topN=10
    ///
    /// Doanh số chỉ tính từ các đơn có trạng thái COMPLETED.
    /// 
    /// Returns:
    /// - CategoryId, Name, Description, Level
    /// - TotalItemsSold: Số lượng items bán từ category
    /// - TotalSalesRevenue: Tổng doanh số (VND)
    /// - PublishedProductCount: Số sản phẩm published trong category
    /// </remarks>
    /// <param name="topN">Số lượng categories muốn lấy (default: 10, max: 50)</param>
    /// <returns>Top categories with sales metrics</returns>
    [HttpGet("top-categories")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopCategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopCategoryDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<IEnumerable<TopCategoryDto>>>> GetTopCategories([FromQuery] int topN = 10)
    {
        // Validate topN parameter
        topN = Math.Max(1, Math.Min(topN, 50)); // Clamp between 1 and 50

        var result = await _analyticsService.GetTopSellCategoriesAsync(topN);

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy top N products bán chạy nhất trong một category
    /// Bao gồm thông tin sản phẩm và metrics bán hàng
    /// </summary>
    /// <remarks>
    /// GET /api/analytics/top-products-by-category/{categoryId}?topN=10
    ///
    /// Doanh số chỉ tính từ các đơn có trạng thái COMPLETED.
    /// 
    /// Returns:
    /// - ProductId, Name, Description, Status
    /// - CategoryId, CategoryName, ShopId
    /// - TotalItemsSold: Số lượng items bán
    /// - TotalSalesRevenue: Tổng doanh số (VND)
    /// - AveragePrice: Giá bán trung bình (VND)
    /// - ThumbnailUrl: URL ảnh đại diện
    /// </remarks>
    /// <param name="categoryId">Category ID</param>
    /// <param name="topN">Số lượng products muốn lấy (default: 10, max: 50)</param>
    /// <returns>Top products in category with sales info</returns>
    [HttpGet("top-products-by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopProductDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopProductDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<IEnumerable<TopProductDto>>>> GetTopProductsByCategory(
        Guid categoryId, 
        [FromQuery] int topN = 10)
    {
        // Validate topN parameter
        topN = Math.Max(1, Math.Min(topN, 50)); // Clamp between 1 and 50

        if (categoryId == Guid.Empty)
            return BadRequest(ServiceResult<IEnumerable<TopProductDto>>.BadRequest("Invalid category ID"));

        var result = await _analyticsService.GetTopProductsByCategoryAsync(categoryId, topN);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy top N products bán chạy nhất (tất cả categories)
    /// Bao gồm thông tin sản phẩm, category, và metrics bán hàng
    /// </summary>
    /// <remarks>
    /// GET /api/analytics/top-products?topN=20
    ///
    /// Doanh số chỉ tính từ các đơn có trạng thái COMPLETED.
    /// 
    /// Returns:
    /// - ProductId, Name, Description, Status
    /// - CategoryId, CategoryName, ShopId
    /// - TotalItemsSold: Số lượng items bán
    /// - TotalSalesRevenue: Tổng doanh số (VND)
    /// - AveragePrice: Giá bán trung bình (VND)
    /// - ThumbnailUrl: URL ảnh đại diện
    /// </remarks>
    /// <param name="topN">Số lượng products muốn lấy (default: 20, max: 100)</param>
    /// <returns>Top products overall with sales info</returns>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<TopProductDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<IEnumerable<TopProductDto>>>> GetTopProducts([FromQuery] int topN = 20)
    {
        // Validate topN parameter
        topN = Math.Max(1, Math.Min(topN, 100)); // Clamp between 1 and 100

        var result = await _analyticsService.GetTopProductsOverallAsync(topN);

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }
}
