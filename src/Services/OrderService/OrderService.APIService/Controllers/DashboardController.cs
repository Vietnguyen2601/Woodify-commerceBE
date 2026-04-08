using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace OrderService.APIService.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Lấy revenue analytics theo Quarterly (Last 4 quarters)
    /// Hiển thị QoQ growth (Quarter-over-Quarter) và YoY growth (Year-over-Year)
    /// </summary>
    /// <remarks>
    /// GET /api/admin/dashboard/revenue/quarterly
    /// 
    /// Returns last 4 quarters of revenue data with:
    /// - Gross Revenue
    /// - Commission Revenue
    /// - Net Revenue
    /// - QoQ Growth (Quarter-over-Quarter)
    /// - YoY Growth (Year-over-Year)
    /// </remarks>
    /// <returns>Quarterly revenue data with growth metrics</returns>
    [HttpGet("revenue/quarterly")]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<RevenueAnalyticsResultDto>>> GetQuarterlyRevenue()
    {
        var result = await _dashboardService.GetQuarterlyRevenueAsync();

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy revenue analytics theo Yearly (Last 3 years)
    /// Hiển thị YoY growth (Year-over-Year trend)
    /// </summary>
    /// <remarks>
    /// GET /api/admin/dashboard/revenue/yearly
    /// 
    /// Returns last 3 years of revenue data with:
    /// - Gross Revenue
    /// - Commission Revenue
    /// - Net Revenue
    /// - YoY Growth (Year-over-Year)
    /// - Projection info for current year
    /// </remarks>
    /// <returns>Yearly revenue data with growth metrics</returns>
    [HttpGet("revenue/yearly")]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<RevenueAnalyticsResultDto>>> GetYearlyRevenue()
    {
        var result = await _dashboardService.GetYearlyRevenueAsync();

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy revenue analytics theo custom date range
    /// Hỗ trợ Daily hoặc Monthly granularity
    /// </summary>
    /// <remarks>
    /// GET /api/admin/dashboard/revenue/custom?startDate=2026-03-01&endDate=2026-04-08&granularity=DAILY
    /// 
    /// Parameters:
    /// - startDate: yyyy-MM-dd (required)
    /// - endDate: yyyy-MM-dd (required)
    /// - granularity: DAILY or MONTHLY (auto-detected if not specified)
    /// 
    /// Returns daily or monthly breakdown of revenue data with:
    /// - Gross Revenue
    /// - Commission Revenue
    /// - Net Revenue
    /// - Growth Rate (compared to previous period)
    /// </remarks>
    /// <param name="startDate">Start date in format yyyy-MM-dd</param>
    /// <param name="endDate">End date in format yyyy-MM-dd</param>
    /// <param name="granularity">DAILY or MONTHLY (optional, auto-detected)</param>
    /// <returns>Custom date range revenue data</returns>
    [HttpGet("revenue/custom")]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<RevenueAnalyticsResultDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<RevenueAnalyticsResultDto>>> GetCustomRevenue(
        [FromQuery(Name = "startDate")] string startDate,
        [FromQuery(Name = "endDate")] string endDate,
        [FromQuery(Name = "granularity")] string? granularity = null)
    {
        var result = await _dashboardService.GetCustomRevenueAsync(startDate, endDate, granularity);

        if (result.Status == 400)
            return BadRequest(result);

        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }
}
