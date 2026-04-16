namespace OrderService.Application.DTOs;

/// <summary>
/// Request DTO cho revenue analytics API
/// </summary>
public class RevenueAnalyticsQueryDto
{
    /// <summary>
    /// Loại time range: LAST_4_QUARTERS, LAST_3_YEARS, CUSTOM
    /// </summary>
    public string TimeRange { get; set; } = "LAST_4_QUARTERS";

    /// <summary>
    /// Granularity: DAILY, MONTHLY, QUARTERLY, YEARLY
    /// Nếu không set, sẽ auto-set dựa trên TimeRange
    /// </summary>
    public string? Granularity { get; set; }

    /// <summary>
    /// Ngày bắt đầu cho CUSTOM range (format: yyyy-MM-dd)
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// Ngày kết thúc cho CUSTOM range (format: yyyy-MM-dd)
    /// </summary>
    public string? EndDate { get; set; }
}

/// <summary>
/// Response DTO cho revenue analytics
/// </summary>
public class RevenueAnalyticsResultDto
{
    public bool Success { get; set; } = true;
    public RevenueAnalyticsDataDto Data { get; set; } = new();
}

/// <summary>
/// Chứa data cho response
/// </summary>
public class RevenueAnalyticsDataDto
{
    public string TimeRange { get; set; } = string.Empty;
    public string Granularity { get; set; } = string.Empty;
    public string Currency { get; set; } = "VND";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string CacheStatus { get; set; } = "MISS"; // HIT hoặc MISS
    public int? CacheTTL { get; set; } // TTL tính bằng giây

    public CustomRangeDto? CustomRange { get; set; }
    
    public RevenueSummaryDto Summary { get; set; } = new();
    public List<RevenueDataPointDto> ChartData { get; set; } = new();
}

/// <summary>
/// Custom date range info
/// </summary>
public class CustomRangeDto
{
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public int Days { get; set; }
}

/// <summary>
/// Summary metrics
/// </summary>
public class RevenueSummaryDto
{
    public long TotalGrossRevenue { get; set; }
    public long TotalCommissionRevenue { get; set; }
    public long TotalNetRevenue { get; set; }
    
    public long AverageDailyRevenue { get; set; }
    public long AverageMonthlyRevenue { get; set; }
    public long AverageQuarterlyRevenue { get; set; }
    public long AverageYearlyRevenue { get; set; }
    
    public decimal AverageCommissionRate { get; set; }
    
    public int TotalOrders { get; set; }
    public long AverageOrderValue { get; set; }
    
    public decimal GrowthRate { get; set; }
    
    public string? BestPeriod { get; set; }
    public string? WorstPeriod { get; set; }
}

/// <summary>
/// Một data point trong chart (hỗ trợ Daily, Monthly, Quarterly, Yearly)
/// </summary>
public class RevenueDataPointDto
{
    // Cho Daily
    public string? Date { get; set; }
    
    // Cho Monthly
    public string? Period { get; set; }
    public int? Month { get; set; }
    
    // Cho Quarterly
    public int? Quarter { get; set; }
    
    // Cho Yearly
    public int? Year { get; set; }
    
    // Revenue metrics
    public long GrossRevenue { get; set; }
    public long CommissionRevenue { get; set; }
    public long NetRevenue { get; set; }
    
    // Order metrics
    public int OrderCount { get; set; }
    public long AverageOrderValue { get; set; }
    
    // Rate & Growth
    public decimal CommissionRate { get; set; }
    public decimal? GrowthRate { get; set; }
    public decimal? QoQGrowth { get; set; }
    public decimal? YoYGrowth { get; set; }
    
    // Projection info
    public bool IsProjected { get; set; } = false;
    public string? ProjectionNote { get; set; }
}

/// <summary>
/// Real-time metrics DTO cho SignalR (phía frontend nhận)
/// Cập nhật mỗi 5 giây
/// </summary>
public class RealtimeMetricsDto
{
    /// <summary>
    /// Timestamp khi data được generate
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ─── TODAY'S METRICS ───
    /// <summary>
    /// Tổng doanh thu hôm nay (từ 00:00 UTC) - tính bằng VND
    /// </summary>
    public decimal GrossRevenue { get; set; }

    /// <summary>
    /// Hoa hồng doanh thu hôm nay - tính bằng VND
    /// </summary>
    public decimal CommissionRevenue { get; set; }

    /// <summary>
    /// Doanh thu ròng hôm nay (Gross - Commission) - tính bằng VND
    /// </summary>
    public decimal NetRevenue { get; set; }

    /// <summary>
    /// Mức tăng trưởng so với hôm qua (%)
    /// </summary>
    public decimal? GrossRevenueGrowth { get; set; }

    /// <summary>
    /// Mức tăng trưởng commission so với hôm qua (%)
    /// </summary>
    public decimal? CommissionGrowth { get; set; }

    /// <summary>
    /// Mức tăng trưởng net revenue so với hôm qua (%)
    /// </summary>
    public decimal? NetRevenueGrowth { get; set; }

    // ─── USER STATISTICS ───
    /// <summary>
    /// Tổng số user trên hệ thống
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Số user mới trong hôm nay
    /// </summary>
    public int NewUsers { get; set; }

    /// <summary>
    /// Mức tăng % user mới so với hôm qua
    /// </summary>
    public decimal? NewUsersGrowth { get; set; }

    /// <summary>
    /// Số user active trong hôm nay (tính từ 00:00 UTC)
    /// </summary>
    public int ActiveUsersToday { get; set; }

    // ─── ORDER STATISTICS ───
    /// <summary>
    /// Số order mới trong hôm nay
    /// </summary>
    public int OrdersToday { get; set; }

    /// <summary>
    /// Số order đã hoàn thành hôm nay
    /// </summary>
    public int CompletedOrdersToday { get; set; }

    /// <summary>
    /// Số order pending (chưa xử lý)
    /// </summary>
    public int PendingOrdersCount { get; set; }

    /// <summary>
    /// Giá trị trung bình mỗi order hôm nay
    /// </summary>
    public decimal? AverageOrderValueToday { get; set; }

    // ─── CACHE & STATUS ───
    /// <summary>
    /// Thông tin cache (HIT hoặc MISS)
    /// </summary>
    public string CacheStatus { get; set; } = "MISS";

    /// <summary>
    /// Time-to-live của cached data (giây)
    /// </summary>
    public int? CacheTTL { get; set; }
}
