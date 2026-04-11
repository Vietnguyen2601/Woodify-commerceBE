using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;

namespace ShopService.APIService.Controllers;

/// <summary>
/// Dashboard Controller
/// Cung cấp API endpoints cho dashboard metrics
/// Bao gồm: realtime metrics, analytics, comparisons, breakdowns
/// 
/// Endpoint pattern: /api/shop/dashboard/{shopId}/...
/// </summary>
[ApiController]
[Route("api/shop/dashboard")]
[AllowAnonymous] // TODO: Thêm authorization sau khi hoàn thiện auth system
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    // ============================================
    // Realtime Metrics Endpoints
    // ============================================

    /// <summary>
    /// Lấy dashboard realtime hoàn chỉnh
    /// Bao gồm: TaskBoard, KPI, Revenue Trend (8 days), Top Products (10)
    /// Cache: 30 giây
    /// 
    /// GET /api/shop/dashboard/{shopId}/metrics
    /// </summary>
    [HttpGet("{shopId}/metrics")]
    [ProducesResponseType(typeof(ServiceResult<ShopDashboardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<ShopDashboardMetricsDto>>> GetMetrics(Guid shopId)
    {
        try
        {
            var metrics = await _dashboardService.GetCompleteMetricsAsync(shopId);
            return Ok(new ServiceResult<ShopDashboardMetricsDto>
            {
                Status = 200,
                Message = "Dashboard metrics retrieved successfully",
                Data = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard metrics for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching dashboard metrics: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lấy task board metrics (danh sách công việc cần làm)
    /// 
    /// GET /api/shop/dashboard/{shopId}/taskboard
    /// </summary>
    [HttpGet("{shopId}/taskboard")]
    [ProducesResponseType(typeof(ServiceResult<TaskBoardMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<TaskBoardMetricsDto>>> GetTaskBoard(Guid shopId)
    {
        try
        {
            var taskBoard = await _dashboardService.GetTaskBoardMetricsAsync(shopId);
            return Ok(new ServiceResult<TaskBoardMetricsDto>
            {
                Status = 200,
                Message = "Task board metrics retrieved successfully",
                Data = taskBoard
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching task board for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching task board metrics: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lấy KPI metrics (doanh thu, tăng trưởng, chuyển đổi)
    /// 
    /// GET /api/shop/dashboard/{shopId}/kpi
    /// </summary>
    [HttpGet("{shopId}/kpi")]
    [ProducesResponseType(typeof(ServiceResult<ShopKPIMetricsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<ShopKPIMetricsDto>>> GetKPIMetrics(Guid shopId)
    {
        try
        {
            var kpi = await _dashboardService.GetKPIMetricsAsync(shopId);
            return Ok(new ServiceResult<ShopKPIMetricsDto>
            {
                Status = 200,
                Message = "KPI metrics retrieved successfully",
                Data = kpi
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching KPI metrics for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching KPI metrics: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lấy xu hướng doanh thu (N ngày gần nhất)
    /// Default: 8 ngày, limit: 30 ngày
    /// 
    /// GET /api/shop/dashboard/{shopId}/revenue-trend?days=8
    /// </summary>
    [HttpGet("{shopId}/revenue-trend")]
    [ProducesResponseType(typeof(ServiceResult<RevenueOverTimeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueOverTimeDto>>> GetRevenueTrend(Guid shopId, [FromQuery] int days = 8)
    {
        try
        {
            if (days < 1 || days > 30)
                days = 8;

            var data = await _dashboardService.GetRevenueOverTimeAsync(shopId, days);
            return Ok(new ServiceResult<RevenueOverTimeDto>
            {
                Status = 200,
                Message = "Revenue trend retrieved successfully",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching revenue trend for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching revenue trend: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lấy top products bán chạy
    /// Default: 10 products, limit: 5-50
    /// Query: limit=10
    /// 
    /// GET /api/shop/dashboard/{shopId}/top-products?limit=10
    /// </summary>
    [HttpGet("{shopId}/top-products")]
    [ProducesResponseType(typeof(ServiceResult<TopProductsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<TopProductsDto>>> GetTopProducts(Guid shopId, [FromQuery] int limit = 10)
    {
        try
        {
            var data = await _dashboardService.GetTopProductsAsync(shopId, limit);
            return Ok(new ServiceResult<TopProductsDto>
            {
                Status = 200,
                Message = "Top products retrieved successfully",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching top products for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching top products: {ex.Message}"
            });
        }
    }

    // ============================================
    // Monthly Analytics Endpoint
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo tháng
    /// Query: year=2026 (default: current year)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/monthly?year=2026
    /// </summary>
    [HttpGet("{shopId}/analytics/monthly")]
    [ProducesResponseType(typeof(ServiceResult<RevenueMonthlyAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueMonthlyAnalyticsDto>>> GetMonthlyRevenue(
        Guid shopId, [FromQuery] int? year)
    {
        try
        {
            var actualYear = year ?? DateTime.UtcNow.Year;
            var result = await _dashboardService.GetMonthlyRevenueAsync(shopId, actualYear);
            return Ok(new ServiceResult<RevenueMonthlyAnalyticsDto>
            {
                Status = 200,
                Message = "Monthly revenue analytics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching monthly revenue for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching monthly revenue: {ex.Message}"
            });
        }
    }

    // ============================================
    // Quarterly Analytics Endpoint
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo quý
    /// Query: year=2026 (default: current year)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/quarterly?year=2026
    /// </summary>
    [HttpGet("{shopId}/analytics/quarterly")]
    [ProducesResponseType(typeof(ServiceResult<RevenueQuarterlyAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueQuarterlyAnalyticsDto>>> GetQuarterlyRevenue(
        Guid shopId, [FromQuery] int? year)
    {
        try
        {
            var actualYear = year ?? DateTime.UtcNow.Year;
            var result = await _dashboardService.GetQuarterlyRevenueAsync(shopId, actualYear);
            return Ok(new ServiceResult<RevenueQuarterlyAnalyticsDto>
            {
                Status = 200,
                Message = "Quarterly revenue analytics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching quarterly revenue for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching quarterly revenue: {ex.Message}"
            });
        }
    }

    // ============================================
    // Yearly Analytics Endpoint
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo năm
    /// Query: startYear=2024, endYear=2026 (defaults: 2 years ago to current year)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/yearly?startYear=2024&endYear=2026
    /// </summary>
    [HttpGet("{shopId}/analytics/yearly")]
    [ProducesResponseType(typeof(ServiceResult<RevenueYearlyAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueYearlyAnalyticsDto>>> GetYearlyRevenue(
        Guid shopId, [FromQuery] int? startYear, [FromQuery] int? endYear)
    {
        try
        {
            var actualStart = startYear ?? DateTime.UtcNow.Year - 2;
            var actualEnd = endYear ?? DateTime.UtcNow.Year;

            if (actualStart > actualEnd)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start year cannot be greater than end year"
                });

            var result = await _dashboardService.GetYearlyRevenueAsync(shopId, actualStart, actualEnd);
            return Ok(new ServiceResult<RevenueYearlyAnalyticsDto>
            {
                Status = 200,
                Message = "Yearly revenue analytics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching yearly revenue for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching yearly revenue: {ex.Message}"
            });
        }
    }

    // ============================================
    // Custom Date Range Analytics
    // ============================================

    /// <summary>
    /// Phân tích doanh thu cho khoảng thời gian tùy chọn
    /// Query: startDate=2026-01-01 (required), endDate=2026-04-10 (required), groupBy=daily|weekly|monthly (default: daily)
    /// Limit: max 365 days
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/custom?startDate=2026-01-01&endDate=2026-04-10&groupBy=daily
    /// </summary>
    [HttpGet("{shopId}/analytics/custom")]
    [ProducesResponseType(typeof(ServiceResult<RevenueCustomAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueCustomAnalyticsDto>>> GetCustomRangeRevenue(
        Guid shopId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string groupBy = "daily")
    {
        try
        {
            if (startDate > endDate)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date"
                });

            if ((endDate - startDate).TotalDays > 365)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Date range cannot exceed 1 year"
                });

            var validGroupBy = new[] { "daily", "weekly", "monthly" };
            if (!validGroupBy.Contains(groupBy.ToLower()))
                groupBy = "daily";

            var result = await _dashboardService.GetCustomRangeRevenueAsync(shopId, startDate, endDate, groupBy);
            return Ok(new ServiceResult<RevenueCustomAnalyticsDto>
            {
                Status = 200,
                Message = "Custom range revenue analytics retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching custom range revenue for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching custom range revenue: {ex.Message}"
            });
        }
    }

    // ============================================
    // Comparison Analytics
    // ============================================

    /// <summary>
    /// So sánh doanh thu giữa 2 khoảng thời gian
    /// Query: period1Start, period1End, period2Start, period2End (all required, YYYY-MM-DD format)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/comparison?period1Start=2026-03-01&period1End=2026-03-31&period2Start=2026-02-01&period2End=2026-02-28
    /// </summary>
    [HttpGet("{shopId}/analytics/comparison")]
    [ProducesResponseType(typeof(ServiceResult<RevenueComparisonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueComparisonDto>>> GetRevenueComparison(
        Guid shopId,
        [FromQuery] DateTime period1Start,
        [FromQuery] DateTime period1End,
        [FromQuery] DateTime period2Start,
        [FromQuery] DateTime period2End)
    {
        try
        {
            if (period1Start > period1End || period2Start > period2End)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date in either period"
                });

            var result = await _dashboardService.GetRevenueComparisonAsync(
                shopId, period1Start, period1End, period2Start, period2End);
            return Ok(new ServiceResult<RevenueComparisonDto>
            {
                Status = 200,
                Message = "Revenue comparison retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching revenue comparison for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching revenue comparison: {ex.Message}"
            });
        }
    }

    // ============================================
    // Summary Analytics
    // ============================================

    /// <summary>
    /// Tóm tắt doanh thu cho khoảng thời gian
    /// Query: timeframe=today|week|month|quarter|year|custom (default: month)
    /// Nếu custom: startDate, endDate (required)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/summary?timeframe=month
    /// GET /api/shop/dashboard/{shopId}/analytics/summary?timeframe=custom&startDate=2026-01-01&endDate=2026-04-10
    /// </summary>
    [HttpGet("{shopId}/analytics/summary")]
    [ProducesResponseType(typeof(ServiceResult<RevenueSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueSummaryDto>>> GetRevenueSummary(
        Guid shopId,
        [FromQuery] string timeframe = "month",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (timeframe.ToLower() == "custom" && (!startDate.HasValue || !endDate.HasValue))
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start and end dates are required for custom timeframe"
                });

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date"
                });

            var result = await _dashboardService.GetRevenueSummaryAsync(shopId, timeframe, startDate, endDate);
            return Ok(new ServiceResult<RevenueSummaryDto>
            {
                Status = 200,
                Message = "Revenue summary retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching revenue summary for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching revenue summary: {ex.Message}"
            });
        }
    }

    // ============================================
    // Breakdown Analytics
    // ============================================

    /// <summary>
    /// Doanh thu phân tách theo Category
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/breakdown/category?startDate=2026-01-01&endDate=2026-04-10
    /// </summary>
    [HttpGet("{shopId}/analytics/breakdown/category")]
    [ProducesResponseType(typeof(ServiceResult<RevenueBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueBreakdownDto>>> GetBreakdownByCategory(
        Guid shopId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date"
                });

            var result = await _dashboardService.GetRevenueBreakdownByCategoryAsync(shopId, startDate, endDate);
            return Ok(new ServiceResult<RevenueBreakdownDto>
            {
                Status = 200,
                Message = "Revenue breakdown by category retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breakdown by category for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching breakdown by category: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Doanh thu phân tách theo Product (top 50)
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/breakdown/product?startDate=2026-01-01&endDate=2026-04-10
    /// </summary>
    [HttpGet("{shopId}/analytics/breakdown/product")]
    [ProducesResponseType(typeof(ServiceResult<RevenueBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueBreakdownDto>>> GetBreakdownByProduct(
        Guid shopId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date"
                });

            var result = await _dashboardService.GetRevenueBreakdownByProductAsync(shopId, startDate, endDate);
            return Ok(new ServiceResult<RevenueBreakdownDto>
            {
                Status = 200,
                Message = "Revenue breakdown by product retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breakdown by product for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching breakdown by product: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Doanh thu phân tách theo Order Status
    /// 
    /// GET /api/shop/dashboard/{shopId}/analytics/breakdown/status?startDate=2026-01-01&endDate=2026-04-10
    /// </summary>
    [HttpGet("{shopId}/analytics/breakdown/status")]
    [ProducesResponseType(typeof(ServiceResult<RevenueBreakdownDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<RevenueBreakdownDto>>> GetBreakdownByStatus(
        Guid shopId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest(new ServiceResult<object>
                {
                    Status = 400,
                    Message = "Start date cannot be after end date"
                });

            var result = await _dashboardService.GetRevenueBreakdownByStatusAsync(shopId, startDate, endDate);
            return Ok(new ServiceResult<RevenueBreakdownDto>
            {
                Status = 200,
                Message = "Revenue breakdown by status retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching breakdown by status for shop {ShopId}", shopId);
            return StatusCode(500, new ServiceResult<object>
            {
                Status = 500,
                Message = $"Error fetching breakdown by status: {ex.Message}"
            });
        }
    }
}
