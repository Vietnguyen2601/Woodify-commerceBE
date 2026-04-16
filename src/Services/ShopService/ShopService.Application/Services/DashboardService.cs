using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Infrastructure.Repositories.IRepositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ShopService.Application.Services;

/// <summary>
/// Dashboard Service Implementation
/// Maps repository data (entities/tuples/primitives) to DTOs
/// Handles business logic + caching
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        IShopRepository shopRepository,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _shopRepository = shopRepository;
        _logger = logger;
    }

    // ============================================
    // Main Dashboard (Realtime)
    // ============================================

    public async Task<ShopDashboardMetricsDto> GetCompleteMetricsAsync(Guid shopId)
    {
        try
        {
            // 1. Check cache first
            var cachedJson = await _dashboardRepository.GetCachedMetricsAsync(shopId);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                _logger.LogDebug("Dashboard metrics retrieved from cache for shop {ShopId}", shopId);
                return JsonSerializer.Deserialize<ShopDashboardMetricsDto>(cachedJson) 
                       ?? throw new Exception("Failed to deserialize cached metrics");
            }

            // 2. Get shop info
            var shop = await _shopRepository.GetByIdAsync(shopId);
            if (shop == null)
            {
                _logger.LogWarning("Shop {ShopId} not found", shopId);
                throw new Exception($"Shop {shopId} not found");
            }

            // 3. Build metrics from repository queries
            var taskBoard = await GetTaskBoardMetricsAsync(shopId);
            var kpi = await GetKPIMetricsAsync(shopId);
            var revenue = await GetRevenueOverTimeAsync(shopId, 8);
            var topProducts = await GetTopProductsAsync(shopId, 10);

            // 4. Aggregate
            var metrics = new ShopDashboardMetricsDto
            {
                ShopId = shopId,
                ShopName = shop.Name,
                TaskBoard = taskBoard,
                KPIMetrics = kpi,
                RevenueOverTime = revenue,
                TopProducts = topProducts,
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Cache for 30 seconds (as JSON string)
            var metricsJson = JsonSerializer.Serialize(metrics);
            await _dashboardRepository.SetCachedMetricsAsync(shopId, metricsJson);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting complete metrics for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<TaskBoardMetricsDto> GetTaskBoardMetricsAsync(Guid shopId)
    {
        try
        {
            var taskBoard = new TaskBoardMetricsDto
            {
                AwaitingPickupCount = await _dashboardRepository.GetAwaitingPickupCountAsync(shopId),
                ReadyToShipCount = await _dashboardRepository.GetReadyToShipCountAsync(shopId),
                ReturnsRefundsCount = await _dashboardRepository.GetReturnsRefundsCountAsync(shopId),
                LockedProductsCount = await _dashboardRepository.GetLockedProductsCountAsync(shopId),
                OrdersSLAViolationCount = await _dashboardRepository.GetOrdersSLAViolationCountAsync(shopId),
                Timestamp = DateTime.UtcNow
            };

            return taskBoard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task board metrics for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<ShopKPIMetricsDto> GetKPIMetricsAsync(Guid shopId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var lastWeekSameDay = today.AddDays(-7);

            // Get today and yesterday revenue
            var todayRevenue = await _dashboardRepository.GetRevenueByDateAsync(shopId, today);
            var yesterdayRevenue = await _dashboardRepository.GetRevenueByDateAsync(shopId, yesterday);

            var revenueGrowth = yesterdayRevenue > 0
                ? ((todayRevenue - yesterdayRevenue) / (decimal)yesterdayRevenue) * 100
                : 0;

            // Get page views
            var todayPageViews = await _dashboardRepository.GetPageViewsAsync(shopId, today);
            var lastWeekPageViews = await _dashboardRepository.GetPageViewsAsync(shopId, lastWeekSameDay);

            var pageViewsGrowth = lastWeekPageViews > 0
                ? ((todayPageViews - lastWeekPageViews) / (decimal)lastWeekPageViews) * 100
                : 0;

            // Conversion rate
            var conversionRate = await _dashboardRepository.GetConversionRateAsync(shopId, today);

            // Orders awaiting action
            var ordersAwaitingAction = await _dashboardRepository.GetOrdersAwaitingActionCountAsync(shopId);

            // SLA violations
            var ordersSLAViolated = await _dashboardRepository.GetOrdersSLAViolationCountAsync(shopId);

            var kpi = new ShopKPIMetricsDto
            {
                RevenueToday = todayRevenue,
                RevenueGrowthPercent = revenueGrowth,
                PageViewsToday = todayPageViews,
                PageViewsGrowthPercent = pageViewsGrowth,
                ConversionRate = conversionRate,
                ConversionRateChangePercent = 0,
                OrdersAwaitingAction = ordersAwaitingAction,
                OrdersSLAViolated = ordersSLAViolated,
                Timestamp = DateTime.UtcNow
            };

            return kpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KPI metrics for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<RevenueOverTimeDto> GetRevenueOverTimeAsync(Guid shopId, int days = 8)
    {
        try
        {
            // Get entities from repository
            var entities = await _dashboardRepository.GetRevenueLastNDaysAsync(shopId, days);

            if (!entities.Any())
            {
                return new RevenueOverTimeDto
                {
                    DailyRevenue = new List<RevenueTimeSeriesDto>(),
                    TotalRevenue = 0,
                    TotalOrders = 0,
                    AverageDailyRevenue = 0,
                    StartDate = DateTime.UtcNow.Date.AddDays(-(days - 1)),
                    EndDate = DateTime.UtcNow.Date
                };
            }

            // Map entities to DTOs
            var dailyRevenue = entities
                .GroupBy(e => e.OrderCompletedAt?.Date ?? DateTime.UtcNow.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueTimeSeriesDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(e => e.NetAmountCents),
                    OrdersCount = g.Count(),
                    CompletedOrdersCount = g.Count(e => e.OrderCompletedAt.HasValue)
                })
                .ToList();

            var total = dailyRevenue.Sum(x => x.Revenue);
            var average = (decimal)total / days;

            return new RevenueOverTimeDto
            {
                DailyRevenue = dailyRevenue,
                TotalRevenue = total,
                TotalOrders = dailyRevenue.Sum(x => x.OrdersCount),
                AverageDailyRevenue = average,
                StartDate = dailyRevenue.FirstOrDefault()?.Date ?? DateTime.UtcNow.Date,
                EndDate = dailyRevenue.LastOrDefault()?.Date ?? DateTime.UtcNow.Date
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue over time for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<TopProductsDto> GetTopProductsAsync(Guid shopId, int limit = 10)
    {
        try
        {
            if (limit < 5) limit = 5;
            if (limit > 50) limit = 50;

            var today = DateTime.UtcNow.Date;
            var monthStart = today.AddDays(-(today.Day - 1));

            // Get entities from repository - returns list of OrderMetricsSnapshot
            // We'll extract product info from here (limited by current OrderMetricsSnapshot structure)
            var entities = await _dashboardRepository.GetTopProductsByShopAsync(shopId, monthStart, today, limit);

            // Map to TopProductMetricDto
            var topProducts = entities
                .Select(e => new TopProductMetricDto
                {
                    ProductId = Guid.NewGuid(), // TODO: Need ProductId in OrderMetricsSnapshot
                    ProductName = $"Product",  // TODO: Need ProductName in OrderMetricsSnapshot
                    SalesCount = e.ItemCount,
                    TotalRevenue = e.NetAmountCents,
                    AverageRating = 0, // Not tracked in snapshot
                    ReviewCount = 0,   // Not tracked in snapshot
                    InventoryCount = 0, // Not tracked in snapshot
                    Rank = 0           // Will be set below
                })
                .OrderByDescending(p => p.SalesCount)
                .Select((p, index) => new TopProductMetricDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    SalesCount = p.SalesCount,
                    TotalRevenue = p.TotalRevenue,
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount,
                    InventoryCount = p.InventoryCount,
                    Rank = index + 1
                })
                .ToList();

            return new TopProductsDto
            {
                TopProducts = topProducts,
                PeriodStart = monthStart,
                PeriodEnd = today,
                TotalProductsSold = topProducts.Sum(x => x.SalesCount)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Monthly Analytics
    // ============================================

    public async Task<RevenueMonthlyAnalyticsDto> GetMonthlyRevenueAsync(Guid shopId, int year)
    {
        try
        {
            // Get tuples from repository: (Month, MonthName, Revenue, OrderCount, CompletedCount, AvgOrderValue)
            var tuples = await _dashboardRepository.GetMonthlyRevenueAsync(shopId, year);

            // Map tuples to DTOs
            var monthlyData = tuples
                .Select(t => new MonthlyRevenueDto
                {
                    Month = t.Month,
                    MonthName = t.MonthName,
                    Revenue = t.Revenue,
                    OrdersCount = t.OrderCount,
                    CompletedOrdersCount = t.CompletedCount,
                    AvgOrderValue = (long)t.AvgOrderValue
                })
                .ToList();

            var totalRevenue = monthlyData.Sum(m => m.Revenue);
            var highestMonth = monthlyData.OrderByDescending(m => m.Revenue).FirstOrDefault()?.Month ?? 1;
            var lowestMonth = monthlyData.OrderBy(m => m.Revenue).FirstOrDefault()?.Month ?? 1;

            return new RevenueMonthlyAnalyticsDto
            {
                Year = year,
                MonthlyData = monthlyData,
                TotalRevenue = totalRevenue,
                TotalOrders = monthlyData.Sum(m => m.OrdersCount),
                AverageMonthlyRevenue = monthlyData.Count > 0 ? (decimal)totalRevenue / monthlyData.Count : 0,
                HighestRevenueMonth = highestMonth,
                LowestRevenueMonth = lowestMonth
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly revenue for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Quarterly Analytics
    // ============================================

    public async Task<RevenueQuarterlyAnalyticsDto> GetQuarterlyRevenueAsync(Guid shopId, int year)
    {
        try
        {
            // Get tuples from repository: (Quarter, QuarterName, Revenue, OrderCount, AvgMonthly)
            var tuples = await _dashboardRepository.GetQuarterlyRevenueAsync(shopId, year);

            // Map tuples to DTOs
            var quarterlyData = tuples
                .Select(t => new QuarterlyRevenueDto
                {
                    Quarter = t.Quarter,
                    QuarterName = t.QuarterName,
                    Revenue = t.Revenue,
                    OrdersCount = t.OrderCount,
                    AvgMonthlyInQuarter = t.AvgMonthly,
                    GrowthPercent = 0 // TODO: Calculate from previous quarter
                })
                .ToList();

            var totalRevenue = quarterlyData.Sum(q => q.Revenue);
            var highestQuarter = quarterlyData.OrderByDescending(q => q.Revenue).FirstOrDefault();
            var lowestQuarter = quarterlyData.OrderBy(q => q.Revenue).FirstOrDefault();

            return new RevenueQuarterlyAnalyticsDto
            {
                Year = year,
                QuarterlyData = quarterlyData,
                TotalRevenue = totalRevenue,
                HighestQuarter = highestQuarter,
                LowestQuarter = lowestQuarter
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quarterly revenue for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Yearly Analytics
    // ============================================

    public async Task<RevenueYearlyAnalyticsDto> GetYearlyRevenueAsync(Guid shopId, int startYear, int endYear)
    {
        try
        {
            // Get tuples from repository: (Year, Revenue, OrderCount, AvgMonthly)
            var tuples = await _dashboardRepository.GetYearlyRevenueAsync(shopId, startYear, endYear);

            // Map tuples to DTOs
            var yearlyData = tuples
                .Select(t => new YearlyRevenueDto
                {
                    Year = t.Year,
                    Revenue = t.Revenue,
                    OrdersCount = t.OrderCount,
                    AvgMonthlyRevenue = t.AvgMonthly,
                    GrowthPercent = 0 // TODO: Calculate from previous year
                })
                .ToList();

            var totalRevenue = yearlyData.Sum(y => y.Revenue);
            var averageYearly = yearlyData.Count > 0 ? (decimal)totalRevenue / yearlyData.Count : 0;
            var highestYear = yearlyData.OrderByDescending(y => y.Revenue).FirstOrDefault();
            var lowestYear = yearlyData.OrderBy(y => y.Revenue).FirstOrDefault();

            return new RevenueYearlyAnalyticsDto
            {
                StartYear = startYear,
                EndYear = endYear,
                YearlyData = yearlyData,
                TotalRevenue = totalRevenue,
                AverageYearlyRevenue = averageYearly,
                HighestYear = highestYear,
                LowestYear = lowestYear
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yearly revenue for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Custom Date Range Analytics
    // ============================================

    public async Task<RevenueCustomAnalyticsDto> GetCustomRangeRevenueAsync(
        Guid shopId, DateTime startDate, DateTime endDate, string groupBy = "daily")
    {
        try
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            if ((endDate - startDate).TotalDays > 365)
                throw new ArgumentException("Date range cannot exceed 1 year");

            // Get entities from repository
            var entities = await _dashboardRepository.GetRevenueGroupedByDateAsync(
                shopId, startDate, endDate, groupBy);

            // Map entities to DTOs
            var revenueData = entities
                .Select(e => new CustomDateRevenueDto
                {
                    Date = e.OrderCompletedAt?.Date ?? DateTime.UtcNow.Date,
                    Revenue = e.NetAmountCents,
                    OrdersCount = e.ItemCount,
                    DailyAverage = 0 // Will calculate below
                })
                .ToList();

            var totalRevenue = revenueData.Sum(r => r.Revenue);
            var totalOrders = revenueData.Sum(r => r.OrdersCount);
            var daysDiff = (endDate.Date - startDate.Date).Days + 1;

            return new RevenueCustomAnalyticsDto
            {
                StartDate = startDate,
                EndDate = endDate,
                GroupBy = groupBy,
                RevenueData = revenueData,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageRevenue = daysDiff > 0 ? (decimal)totalRevenue / daysDiff : 0,
                DaysInRange = daysDiff
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting custom range revenue for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Comparison Analytics
    // ============================================

    public async Task<RevenueComparisonDto> GetRevenueComparisonAsync(
        Guid shopId, DateTime period1Start, DateTime period1End,
        DateTime period2Start, DateTime period2End)
    {
        try
        {
            if (period1Start > period1End || period2Start > period2End)
                throw new ArgumentException("Start date cannot be after end date");

            var period1Revenue = await _dashboardRepository.GetRevenueByDateRangeAsync(
                shopId, period1Start, period1End);
            var period1StatusDict = await _dashboardRepository.GetOrdersCountByStatusAsync(
                shopId, period1Start, period1End);
            var period1Orders = period1StatusDict.Values.Sum();

            var period2Revenue = await _dashboardRepository.GetRevenueByDateRangeAsync(
                shopId, period2Start, period2End);
            var period2StatusDict = await _dashboardRepository.GetOrdersCountByStatusAsync(
                shopId, period2Start, period2End);
            var period2Orders = period2StatusDict.Values.Sum();

            var days1 = (period1End.Date - period1Start.Date).Days + 1;
            var days2 = (period2End.Date - period2Start.Date).Days + 1;

            var p1 = new PeriodComparisonDto
            {
                PeriodStart = period1Start,
                PeriodEnd = period1End,
                DaysCount = days1,
                TotalRevenue = period1Revenue,
                OrdersCount = period1Orders,
                AverageDailyRevenue = days1 > 0 ? (decimal)period1Revenue / days1 : 0
            };

            var p2 = new PeriodComparisonDto
            {
                PeriodStart = period2Start,
                PeriodEnd = period2End,
                DaysCount = days2,
                TotalRevenue = period2Revenue,
                OrdersCount = period2Orders,
                AverageDailyRevenue = days2 > 0 ? (decimal)period2Revenue / days2 : 0
            };

            var revenueDiff = p1.TotalRevenue - p2.TotalRevenue;
            var revenuePercent = p2.TotalRevenue > 0
                ? (revenueDiff / (decimal)p2.TotalRevenue) * 100
                : 0;

            var ordersDiff = p1.OrdersCount - p2.OrdersCount;
            var ordersPercent = p2.OrdersCount > 0
                ? (ordersDiff / (decimal)p2.OrdersCount) * 100
                : 0;

            var summary = revenuePercent >= 0
                ? $"Period 1 increased by {revenuePercent:F1}% compared to Period 2"
                : $"Period 1 decreased by {Math.Abs(revenuePercent):F1}% compared to Period 2";

            return new RevenueComparisonDto
            {
                Period1 = p1,
                Period2 = p2,
                RevenueDifference = revenueDiff,
                RevenueChangePercent = revenuePercent,
                OrdersDifference = ordersDiff,
                OrdersChangePercent = ordersPercent,
                ComparisonSummary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue comparison for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Summary Analytics
    // ============================================

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        Guid shopId, string timeframe = "month",
        DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // Determine date range based on timeframe
            (DateTime actualStart, DateTime actualEnd) = timeframe.ToLower() switch
            {
                "today" => (DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1)),
                "week" => (DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek), DateTime.UtcNow.Date),
                "month" => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow.Date),
                "quarter" => GetQuarterDates(DateTime.UtcNow),
                "year" => (new DateTime(DateTime.UtcNow.Year, 1, 1), DateTime.UtcNow.Date),
                "custom" => (startDate ?? DateTime.UtcNow.Date, endDate ?? DateTime.UtcNow.Date),
                _ => (DateTime.UtcNow.Date.AddMonths(-1), DateTime.UtcNow.Date)
            };

            var totalRevenue = await _dashboardRepository.GetRevenueByDateRangeAsync(
                shopId, actualStart, actualEnd);
            var netRevenue = await _dashboardRepository.GetNetRevenueByDateRangeAsync(
                shopId, actualStart, actualEnd);
            var commission = totalRevenue - netRevenue;

            var ordersByStatus = await _dashboardRepository.GetOrdersCountByStatusAsync(
                shopId, actualStart, actualEnd);

            var daysDiff = (actualEnd.Date - actualStart.Date).Days + 1;
            var topProductsEntities = await _dashboardRepository.GetTopProductsByShopAsync(
                shopId, actualStart, actualEnd, 5);

            var topProducts = topProductsEntities
                .Select(e => new TopProductMetricDto
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product",
                    SalesCount = e.ItemCount,
                    TotalRevenue = e.NetAmountCents,
                    AverageRating = 0,
                    ReviewCount = 0,
                    InventoryCount = 0,
                    Rank = 0
                })
                .ToList();

            // Calculate growth
            var prevStart = actualStart.AddDays(-daysDiff);
            var prevEnd = actualStart.AddSeconds(-1);
            var prevRevenue = await _dashboardRepository.GetRevenueByDateRangeAsync(
                shopId, prevStart, prevEnd);

            var growthPercent = prevRevenue > 0
                ? ((totalRevenue - prevRevenue) / (decimal)prevRevenue) * 100
                : 0;

            var totalOrders = ordersByStatus.Values.Sum();

            return new RevenueSummaryDto
            {
                Timeframe = timeframe,
                StartDate = actualStart,
                EndDate = actualEnd,
                TotalRevenue = totalRevenue,
                NetRevenue = netRevenue,
                CommissionTotal = commission,
                TotalOrders = totalOrders,
                CompletedOrders = ordersByStatus.TryGetValue("COMPLETED", out var completed) ? completed : 0,
                PendingOrders = ordersByStatus.TryGetValue("PENDING", out var pending) ? pending : 0,
                CancelledOrders = ordersByStatus.TryGetValue("CANCELLED", out var cancelled) ? cancelled : 0,
                AverageDailyRevenue = daysDiff > 0 ? (decimal)totalRevenue / daysDiff : 0,
                AverageOrderValue = totalOrders > 0 ? (decimal)totalRevenue / totalOrders : 0,
                GrowthPercent = growthPercent,
                TopProducts = topProducts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue summary for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Breakdown Analytics
    // ============================================

    public async Task<RevenueBreakdownDto> GetRevenueBreakdownByCategoryAsync(
        Guid shopId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            var tuples = await _dashboardRepository.GetRevenueBreakdownByCategoryAsync(
                shopId, startDate, endDate);

            var totalRevenue = tuples.Sum(t => t.Revenue);

            var ranked = tuples
                .Select((item, index) => new RevenueBreakdownItemDto
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ItemName = item.Category,
                    Revenue = item.Revenue,
                    OrdersCount = item.OrderCount,
                    Percentage = totalRevenue > 0 ? (item.Percentage) : 0,
                    Rank = index + 1
                })
                .ToList();

            return new RevenueBreakdownDto
            {
                StartDate = startDate,
                EndDate = endDate,
                BreakdownBy = "category",
                BreakdownData = ranked,
                TotalRevenue = totalRevenue,
                TotalItems = ranked.Count,
                TopItem = ranked.FirstOrDefault()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue breakdown by category for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<RevenueBreakdownDto> GetRevenueBreakdownByProductAsync(
        Guid shopId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            var tuples = await _dashboardRepository.GetRevenueBreakdownByProductAsync(
                shopId, startDate, endDate);

            var totalRevenue = tuples.Sum(t => t.Revenue);

            var ranked = tuples
                .Select((item, index) => new RevenueBreakdownItemDto
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ItemName = item.ProductName,
                    Revenue = item.Revenue,
                    OrdersCount = item.OrderCount,
                    Percentage = totalRevenue > 0 ? (item.Percentage) : 0,
                    Rank = index + 1
                })
                .ToList();

            return new RevenueBreakdownDto
            {
                StartDate = startDate,
                EndDate = endDate,
                BreakdownBy = "product",
                BreakdownData = ranked,
                TotalRevenue = totalRevenue,
                TotalItems = ranked.Count,
                TopItem = ranked.FirstOrDefault()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue breakdown by product for shop {ShopId}", shopId);
            throw;
        }
    }

    public async Task<RevenueBreakdownDto> GetRevenueBreakdownByStatusAsync(
        Guid shopId, DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            var tuples = await _dashboardRepository.GetRevenueBreakdownByOrderStatusAsync(
                shopId, startDate, endDate);

            var totalRevenue = tuples.Sum(t => t.Revenue);

            var ranked = tuples
                .Select((item, index) => new RevenueBreakdownItemDto
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ItemName = item.Status,
                    Revenue = item.Revenue,
                    OrdersCount = item.OrderCount,
                    Percentage = totalRevenue > 0 ? (item.Percentage) : 0,
                    Rank = index + 1
                })
                .ToList();

            return new RevenueBreakdownDto
            {
                StartDate = startDate,
                EndDate = endDate,
                BreakdownBy = "orderStatus",
                BreakdownData = ranked,
                TotalRevenue = totalRevenue,
                TotalItems = ranked.Count,
                TopItem = ranked.FirstOrDefault()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue breakdown by status for shop {ShopId}", shopId);
            throw;
        }
    }

    // ============================================
    // Cache Management
    // ============================================

    public async Task RefreshMetricsCacheAsync(Guid shopId)
    {
        try
        {
            await _dashboardRepository.InvalidateCacheAsync(shopId);
            var metrics = await GetCompleteMetricsAsync(shopId);
            _logger.LogInformation("Cache refreshed for shop {ShopId}", shopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache for shop {ShopId}", shopId);
        }
    }

    // ============================================
    // Helper Methods
    // ============================================

    private (DateTime start, DateTime end) GetQuarterDates(DateTime date)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        var startMonth = (quarter - 1) * 3 + 1;
        var start = new DateTime(date.Year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        return (start, end);
    }
}
