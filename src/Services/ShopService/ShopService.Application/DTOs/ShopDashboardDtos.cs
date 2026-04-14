namespace ShopService.Application.DTOs;

/// <summary>
/// Dashboard realtime metrics - Task board section
/// Hiển thị danh sách công việc cần xử lý
/// </summary>
public class TaskBoardMetricsDto
{
    /// <summary>Đơn hàng chờ lấy hàng (< 45 phút SLA)</summary>
    public int AwaitingPickupCount { get; set; }

    /// <summary>Đơn hàng đã xử lý, chuẩn bị giao cho vận chuyển</summary>
    public int ReadyToShipCount { get; set; }

    /// <summary>Đơn hàng chờ xử lý trả/hoàn/hủy (trong 24h)</summary>
    public int ReturnsRefundsCount { get; set; }

    /// <summary>Sản phẩm bị tạm khóa cần kiểm tra</summary>
    public int LockedProductsCount { get; set; }

    /// <summary>Đơn hàng vượt quá SLA (quá hạn)</summary>
    public int OrdersSLAViolationCount { get; set; }

    /// <summary>Thời điểm cập nhật metrics</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dashboard realtime metrics - KPI section
/// Hiển thị các chỉ số quan trọng (doanh thu, lượt truy cập, tỉ lệ chuyển đổi)
/// </summary>
public class ShopKPIMetricsDto
{
    /// <summary>Doanh thu hôm nay (VND, tính bằng đơn vị nhỏ nhất)</summary>
    public long RevenueToday { get; set; }

    /// <summary>Tăng trưởng doanh thu so với hôm qua (%)</summary>
    public decimal RevenueGrowthPercent { get; set; }

    /// <summary>Lượt truy cập shop hôm nay</summary>
    public int PageViewsToday { get; set; }

    /// <summary>Tăng trưởng lượt truy cập so với tuần trước cùng ngày (%)</summary>
    public decimal PageViewsGrowthPercent { get; set; }

    /// <summary>Tỉ lệ chuyển đổi (Orders / PageViews * 100) (%)</summary>
    public decimal ConversionRate { get; set; }

    /// <summary>Thay đổi tỉ lệ chuyển đổi so với tuần trước (%)</summary>
    public decimal ConversionRateChangePercent { get; set; }

    /// <summary>Số đơn hàng chống xử lý (AwaitingPickup + ReadyToShip + Pending)</summary>
    public int OrdersAwaitingAction { get; set; }

    /// <summary>Số đơn hàng vượt quá SLA (quá hạn xử lý)</summary>
    public int OrdersSLAViolated { get; set; }

    /// <summary>Thời điểm cập nhật</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Doanh thu theo ngày (time series)
/// </summary>
public class RevenueTimeSeriesDto
{
    /// <summary>Ngày</summary>
    public DateTime Date { get; set; }

    /// <summary>Doanh thu trong ngày (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng trong ngày</summary>
    public int OrdersCount { get; set; }

    /// <summary>Số đơn hàng hoàn thành</summary>
    public int CompletedOrdersCount { get; set; }
}

/// <summary>
/// Xu hướng doanh thu qua thời gian (biểu đồ)
/// </summary>
public class RevenueOverTimeDto
{
    /// <summary>Doanh thu từng ngày trong khoảng thời gian (8 ngày gần nhất)</summary>
    public List<RevenueTimeSeriesDto> DailyRevenue { get; set; } = new();

    /// <summary>Tổng doanh thu</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Tổng số đơn hàng</summary>
    public int TotalOrders { get; set; }

    /// <summary>Mức doanh thu trung bình/ngày</summary>
    public decimal AverageDailyRevenue { get; set; }

    /// <summary>Ngày bắt đầu khoảng thời gian</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Ngày kết thúc khoảng thời gian</summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Thông tin sản phẩm bán chạy
/// </summary>
public class TopProductMetricDto
{
    /// <summary>ID sản phẩm</summary>
    public Guid ProductId { get; set; }

    /// <summary>Tên sản phẩm</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Số lượng bán (trong khoảng thời gian)</summary>
    public int SalesCount { get; set; }

    /// <summary>Tổng doanh thu từ sản phẩm (VND)</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Đánh giá trung bình</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Số lượng đánh giá/review</summary>
    public int ReviewCount { get; set; }

    /// <summary>Số lượng tồn kho hiện tại</summary>
    public int InventoryCount { get; set; }

    /// <summary>Xếp hạng (1 = bán chạy nhất)</summary>
    public int Rank { get; set; }
}

/// <summary>
/// Top sản phẩm bán chạy
/// </summary>
public class TopProductsDto
{
    /// <summary>Danh sách top sản phẩm (mặc định 10 sản phẩm)</summary>
    public List<TopProductMetricDto> TopProducts { get; set; } = new();

    /// <summary>Ngày bắt đầu khoảng thời gian (thường là đầu tháng)</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Ngày kết thúc khoảng thời gian (thường là hôm nay)</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Tổng số sản phẩm bán được (toàn bộ sản phẩm, không chỉ top)</summary>
    public int TotalProductsSold { get; set; }
}

/// <summary>
/// Dashboard realtime hoàn chỉnh (Bảng điều khiển cửa hàng)
/// Bao gồm: taskboard, KPI metrics, revenue trend, top products
/// </summary>
public class ShopDashboardMetricsDto
{
    /// <summary>ID cửa hàng</summary>
    public Guid ShopId { get; set; }

    /// <summary>Tên cửa hàng</summary>
    public string ShopName { get; set; } = string.Empty;

    /// <summary>Danh sách công việc cần xử lý</summary>
    public TaskBoardMetricsDto TaskBoard { get; set; } = new();

    /// <summary>Chỉ số KPI (doanh thu, tăng trưởng, chuyển đổi)</summary>
    public ShopKPIMetricsDto KPIMetrics { get; set; } = new();

    /// <summary>Xu hướng doanh thu (8 ngày gần nhất)</summary>
    public RevenueOverTimeDto RevenueOverTime { get; set; } = new();

    /// <summary>Top sản phẩm bán chạy</summary>
    public TopProductsDto TopProducts { get; set; } = new();

    /// <summary>Thời điểm cập nhật dashboard</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// ============================================
// Analytics DTOs - Phân tích doanh thu
// ============================================

/// <summary>
/// Doanh thu theo tháng
/// </summary>
public class MonthlyRevenueDto
{
    /// <summary>Số tháng (1-12)</summary>
    public int Month { get; set; }

    /// <summary>Tên tháng</summary>
    public string MonthName { get; set; } = string.Empty;

    /// <summary>Doanh thu trong tháng (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng trong tháng</summary>
    public int OrdersCount { get; set; }

    /// <summary>Số đơn hàng hoàn thành</summary>
    public int CompletedOrdersCount { get; set; }

    /// <summary>Giá trị trung bình mỗi đơn hàng</summary>
    public decimal AvgOrderValue { get; set; }
}

/// <summary>
/// Phân tích doanh thu theo tháng trong 1 năm
/// </summary>
public class RevenueMonthlyAnalyticsDto
{
    /// <summary>Năm</summary>
    public int Year { get; set; }

    /// <summary>Doanh thu từng tháng</summary>
    public List<MonthlyRevenueDto> MonthlyData { get; set; } = new();

    /// <summary>Tổng doanh thu năm</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Tổng số đơn hàng năm</summary>
    public int TotalOrders { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi tháng</summary>
    public decimal AverageMonthlyRevenue { get; set; }

    /// <summary>Số tháng có doanh thu cao nhất</summary>
    public int HighestRevenueMonth { get; set; }

    /// <summary>Số tháng có doanh thu thấp nhất</summary>
    public int LowestRevenueMonth { get; set; }
}

/// <summary>
/// Doanh thu theo quý
/// </summary>
public class QuarterlyRevenueDto
{
    /// <summary>Số quý (1-4)</summary>
    public int Quarter { get; set; }

    /// <summary>Tên quý (Q1, Q2, Q3, Q4)</summary>
    public string QuarterName { get; set; } = string.Empty;

    /// <summary>Doanh thu trong quý (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng trong quý</summary>
    public int OrdersCount { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi tháng trong quý</summary>
    public decimal AvgMonthlyInQuarter { get; set; }

    /// <summary>Tăng trưởng so với quý trước (%)</summary>
    public decimal GrowthPercent { get; set; }
}

/// <summary>
/// Phân tích doanh thu theo quý trong 1 năm
/// </summary>
public class RevenueQuarterlyAnalyticsDto
{
    /// <summary>Năm</summary>
    public int Year { get; set; }

    /// <summary>Doanh thu từng quý</summary>
    public List<QuarterlyRevenueDto> QuarterlyData { get; set; } = new();

    /// <summary>Tổng doanh thu năm</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Quý có doanh thu cao nhất</summary>
    public QuarterlyRevenueDto? HighestQuarter { get; set; }

    /// <summary>Quý có doanh thu thấp nhất</summary>
    public QuarterlyRevenueDto? LowestQuarter { get; set; }
}

/// <summary>
/// Doanh thu theo năm
/// </summary>
public class YearlyRevenueDto
{
    /// <summary>Năm</summary>
    public int Year { get; set; }

    /// <summary>Doanh thu năm đó (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng trong năm</summary>
    public int OrdersCount { get; set; }

    /// <summary>Tăng trưởng so với năm trước (%)</summary>
    public decimal GrowthPercent { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi tháng</summary>
    public decimal AvgMonthlyRevenue { get; set; }
}

/// <summary>
/// Phân tích doanh thu theo năm (nhìn xu hướng nhiều năm)
/// </summary>
public class RevenueYearlyAnalyticsDto
{
    /// <summary>Năm bắt đầu</summary>
    public int StartYear { get; set; }

    /// <summary>Năm kết thúc</summary>
    public int EndYear { get; set; }

    /// <summary>Doanh thu từng năm</summary>
    public List<YearlyRevenueDto> YearlyData { get; set; } = new();

    /// <summary>Tổng doanh thu toàn bộ khoảng thời gian</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi năm</summary>
    public decimal AverageYearlyRevenue { get; set; }

    /// <summary>Năm có doanh thu cao nhất</summary>
    public YearlyRevenueDto? HighestYear { get; set; }

    /// <summary>Năm có doanh thu thấp nhất</summary>
    public YearlyRevenueDto? LowestYear { get; set; }
}

/// <summary>
/// Doanh thu trong khoảng thời gian tùy chọn (daily/weekly/monthly grouping)
/// </summary>
public class CustomDateRevenueDto
{
    /// <summary>Ngày/Tuần/Tháng (phụ thuộc groupBy)</summary>
    public DateTime Date { get; set; }

    /// <summary>Doanh thu (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng</summary>
    public int OrdersCount { get; set; }

    /// <summary>Mức doanh thu trung bình (total / days in range)</summary>
    public decimal DailyAverage { get; set; }
}

/// <summary>
/// Phân tích doanh thu cho khoảng thời gian tùy chọn
/// </summary>
public class RevenueCustomAnalyticsDto
{
    /// <summary>Ngày bắt đầu</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Cách nhóm dữ liệu: daily, weekly, monthly</summary>
    public string GroupBy { get; set; } = "daily";

    /// <summary>Doanh thu theo từng ngày/tuần/tháng</summary>
    public List<CustomDateRevenueDto> RevenueData { get; set; } = new();

    /// <summary>Tổng doanh thu</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Tổng số đơn hàng</summary>
    public int TotalOrders { get; set; }

    /// <summary>Mức doanh thu trung bình</summary>
    public decimal AverageRevenue { get; set; }

    /// <summary>Số ngày trong khoảng thời gian</summary>
    public int DaysInRange { get; set; }
}

/// <summary>
/// Dữ liệu khoảng thời gian để so sánh
/// </summary>
public class PeriodComparisonDto
{
    /// <summary>Ngày bắt đầu khoảng thời gian</summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>Ngày kết thúc khoảng thời gian</summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>Số ngày trong khoảng</summary>
    public int DaysCount { get; set; }

    /// <summary>Tổng doanh thu khoảng này (VND)</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Số đơn hàng</summary>
    public int OrdersCount { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi ngày</summary>
    public decimal AverageDailyRevenue { get; set; }
}

/// <summary>
/// So sánh doanh thu giữa 2 khoảng thời gian
/// </summary>
public class RevenueComparisonDto
{
    /// <summary>Khoảng thời gian thứ nhất</summary>
    public PeriodComparisonDto? Period1 { get; set; }

    /// <summary>Khoảng thời gian thứ hai</summary>
    public PeriodComparisonDto? Period2 { get; set; }

    /// <summary>Chênh lệch doanh thu (Period1 - Period2) (VND)</summary>
    public long RevenueDifference { get; set; }

    /// <summary>Phần trăm thay đổi doanh thu (%)</summary>
    public decimal RevenueChangePercent { get; set; }

    /// <summary>Chênh lệch số đơn hàng</summary>
    public int OrdersDifference { get; set; }

    /// <summary>Phần trăm thay đổi số đơn hàng (%)</summary>
    public decimal OrdersChangePercent { get; set; }

    /// <summary>Tóm tắt so sánh (text)</summary>
    public string ComparisonSummary { get; set; } = string.Empty;
}

/// <summary>
/// Tóm tắt doanh thu cho khoảng thời gian cụ thể
/// </summary>
public class RevenueSummaryDto
{
    /// <summary>Khung thời gian: today, week, month, quarter, year, custom</summary>
    public string Timeframe { get; set; } = string.Empty;

    /// <summary>Ngày bắt đầu</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Tổng doanh thu (VND)</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Doanh thu ròng (sau khi trừ hoa hồng/commission)</summary>
    public long NetRevenue { get; set; }

    /// <summary>Tổng hoa hồng/commission</summary>
    public long CommissionTotal { get; set; }

    /// <summary>Tổng số đơn hàng</summary>
    public int TotalOrders { get; set; }

    /// <summary>Số đơn hàng hoàn thành</summary>
    public int CompletedOrders { get; set; }

    /// <summary>Số đơn hàng đang xử lý</summary>
    public int PendingOrders { get; set; }

    /// <summary>Số đơn hàng bị hủy</summary>
    public int CancelledOrders { get; set; }

    /// <summary>Mức doanh thu trung bình mỗi ngày</summary>
    public decimal AverageDailyRevenue { get; set; }

    /// <summary>Giá trị trung bình mỗi đơn hàng</summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>Tăng trưởng so với khoảng thời gian tương ứng trước đó (%)</summary>
    public decimal GrowthPercent { get; set; }

    /// <summary>Top 5 sản phẩm bán chạy</summary>
    public List<TopProductMetricDto> TopProducts { get; set; } = new();
}

/// <summary>
/// Item trong phân tích phân tách doanh thu (by category, product, status)
/// </summary>
public class RevenueBreakdownItemDto
{
    /// <summary>ID category/product/status</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>Tên category/product/status</summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>Doanh thu (VND)</summary>
    public long Revenue { get; set; }

    /// <summary>Số đơn hàng</summary>
    public int OrdersCount { get; set; }

    /// <summary>Phần trăm so với tổng doanh thu (%)</summary>
    public decimal Percentage { get; set; }

    /// <summary>Xếp hạng (1 = highest revenue)</summary>
    public int Rank { get; set; }
}

/// <summary>
/// Phân tích phân tách doanh thu theo category/product/orderStatus
/// </summary>
public class RevenueBreakdownDto
{
    /// <summary>Ngày bắt đầu</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Ngày kết thúc</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Phân tách theo: category, product, orderStatus</summary>
    public string BreakdownBy { get; set; } = "category";

    /// <summary>Dữ liệu phân tách</summary>
    public List<RevenueBreakdownItemDto> BreakdownData { get; set; } = new();

    /// <summary>Tổng doanh thu</summary>
    public long TotalRevenue { get; set; }

    /// <summary>Tổng số items</summary>
    public int TotalItems { get; set; }

    /// <summary>Item có doanh thu cao nhất</summary>
    public RevenueBreakdownItemDto? TopItem { get; set; }
}
