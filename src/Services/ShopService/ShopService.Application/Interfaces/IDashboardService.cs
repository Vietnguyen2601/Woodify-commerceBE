using ShopService.Application.DTOs;

namespace ShopService.Application.Interfaces;

/// <summary>
/// Dashboard Service Interface
/// Xử lý business logic cho dashboard metrics
/// Cấu trúc:
/// - Realtime metrics từ cache + events aggregation
/// - Analytics từ historical data queries
/// - Integration với OrderService events
/// </summary>
public interface IDashboardService
{
    // ============================================
    // Realtime Metrics (Dashboard Main View)
    // ============================================

    /// <summary>
    /// Lấy dashboard realtime hoàn chỉnh
    /// Bao gồm: TaskBoard, KPI, Revenue Trend, Top Products
    /// Dữ liệu này mỗi 5 giây cập nhật via events
    /// </summary>
    Task<ShopDashboardMetricsDto> GetCompleteMetricsAsync(Guid shopId);

    /// <summary>
    /// Lấy task board metrics (danh sách công việc cần làm)
    /// </summary>
    Task<TaskBoardMetricsDto> GetTaskBoardMetricsAsync(Guid shopId);

    /// <summary>
    /// Lấy KPI metrics (doanh thu, tăng trưởng, chuyển đổi)
    /// </summary>
    Task<ShopKPIMetricsDto> GetKPIMetricsAsync(Guid shopId);

    /// <summary>
    /// Lấy xu hướng doanh thu (N ngày gần nhất, mặc định 8 ngày)
    /// </summary>
    Task<RevenueOverTimeDto> GetRevenueOverTimeAsync(Guid shopId, int days = 8);

    /// <summary>
    /// Lấy top sản phẩm bán chạy (mặc định 10, limit tối đa 50)
    /// </summary>
    Task<TopProductsDto> GetTopProductsAsync(Guid shopId, int limit = 10);

    // ============================================
    // Monthly Analytics
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo tháng trong 1 năm
    /// Kết quả: 12 tháng với revenue, orders, avg order value
    /// </summary>
    Task<RevenueMonthlyAnalyticsDto> GetMonthlyRevenueAsync(Guid shopId, int year);

    // ============================================
    // Quarterly Analytics
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo quý trong 1 năm
    /// Kết quả: 4 quý với revenue, orders, growth %
    /// </summary>
    Task<RevenueQuarterlyAnalyticsDto> GetQuarterlyRevenueAsync(Guid shopId, int year);

    // ============================================
    // Yearly Analytics
    // ============================================

    /// <summary>
    /// Phân tích doanh thu theo năm trong khoảng nhiều năm
    /// Dùng để nhìn xu hướng dài hạn
    /// </summary>
    Task<RevenueYearlyAnalyticsDto> GetYearlyRevenueAsync(Guid shopId, int startYear, int endYear);

    // ============================================
    // Custom Date Range Analytics
    // ============================================

    /// <summary>
    /// Phân tích doanh thu cho khoảng thời gian tùy chọn
    /// Có thể nhóm theo: daily (mặc định), weekly, monthly
    /// </summary>
    Task<RevenueCustomAnalyticsDto> GetCustomRangeRevenueAsync(
        Guid shopId, DateTime startDate, DateTime endDate, string groupBy = "daily");

    // ============================================
    // Comparison Analytics
    // ============================================

    /// <summary>
    /// So sánh doanh thu giữa 2 khoảng thời gian
    /// Kết quả: chênh lệch doanh thu, % thay đổi
    /// </summary>
    Task<RevenueComparisonDto> GetRevenueComparisonAsync(
        Guid shopId, DateTime period1Start, DateTime period1End,
        DateTime period2Start, DateTime period2End);

    // ============================================
    // Summary Analytics
    // ============================================

    /// <summary>
    /// Tóm tắt doanh thu cho khoảng thời gian cụ thể
    /// Timeframe: today, week, month, quarter, year, custom
    /// Bao gồm: doanh thu, số order, growth, top products
    /// </summary>
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        Guid shopId, string timeframe = "month",
        DateTime? startDate = null, DateTime? endDate = null);

    // ============================================
    // Breakdown Analytics
    // ============================================

    /// <summary>
    /// Doanh thu phân tách theo Category trong khoảng thời gian
    /// </summary>
    Task<RevenueBreakdownDto> GetRevenueBreakdownByCategoryAsync(
        Guid shopId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Doanh thu phân tách theo Product trong khoảng thời gian
    /// </summary>
    Task<RevenueBreakdownDto> GetRevenueBreakdownByProductAsync(
        Guid shopId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Doanh thu phân tách theo Order Status trong khoảng thời gian
    /// </summary>
    Task<RevenueBreakdownDto> GetRevenueBreakdownByStatusAsync(
        Guid shopId, DateTime startDate, DateTime endDate);

    // ============================================
    // Cache Management
    // ============================================

    /// <summary>
    /// Refresh dashboard cache (sau khi nhận events từ OrderService)
    /// Không public - chỉ dùng internal từ event consumers
    /// </summary>
    Task RefreshMetricsCacheAsync(Guid shopId);
}
