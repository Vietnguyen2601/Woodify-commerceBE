using OrderService.Application.DTOs;
using Shared.Results;

namespace OrderService.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Lấy revenue analytics theo Quarterly (Last 4 quarters)
    /// Hiển thị QoQ growth và YoY growth
    /// </summary>
    Task<ServiceResult<RevenueAnalyticsResultDto>> GetQuarterlyRevenueAsync();

    /// <summary>
    /// Lấy revenue analytics theo Yearly (Last 3 years)
    /// Hiển thị YoY growth trend
    /// </summary>
    Task<ServiceResult<RevenueAnalyticsResultDto>> GetYearlyRevenueAsync();

    /// <summary>
    /// Lấy revenue analytics theo custom date range
    /// Hỗ trợ Daily hoặc Monthly granularity
    /// </summary>
    Task<ServiceResult<RevenueAnalyticsResultDto>> GetCustomRevenueAsync(
        string startDate,
        string endDate,
        string? granularity = null);
}
