using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Repositories.IRepositories;

namespace ShopService.Infrastructure.Repositories;

/// <summary>
/// Dashboard Repository Implementation
/// Trả về Entities + tuples, không import Application DTOs
/// Service layer làm việc mapping sang DTOs
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly ShopDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CACHE_KEY_PREFIX = "dashboard:";
    private const int CACHE_DURATION_SECONDS = 30;

    public DashboardRepository(ShopDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // ============================================
    // Task Board Queries
    // ============================================

    public async Task<int> GetAwaitingPickupCountAsync(Guid shopId)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   (o.Status == "CONFIRMED" || o.Status == "PROCESSING"))
            .CountAsync();
    }

    public async Task<int> GetReadyToShipCountAsync(Guid shopId)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && o.Status == "READY_TO_SHIP")
            .CountAsync();
    }

    public async Task<int> GetReturnsRefundsCountAsync(Guid shopId)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   (o.IsReturn || 
                    o.Status == "REFUNDED" || 
                    o.Status == "CANCELLED" ||
                    o.Status == "REFUNDING"))
            .CountAsync();
    }

    public async Task<int> GetLockedProductsCountAsync(Guid shopId)
    {
        // TODO: Cần integrate với ProductService để lấy locked products count
        // Tạm thời return 0
        return await Task.FromResult(0);
    }

    public async Task<int> GetOrdersSLAViolationCountAsync(Guid shopId)
    {
        var violationThreshold = DateTime.UtcNow.AddMinutes(-45);
        
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   (o.Status == "CONFIRMED" || o.Status == "PROCESSING" || o.Status == "READY_TO_SHIP") &&
                   o.OrderCreatedAt < violationThreshold)
            .CountAsync();
    }

    // ============================================
    // KPI Queries
    // ============================================

    public async Task<long> GetRevenueByDateAsync(Guid shopId, DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCompletedAt >= startDate &&
                   o.OrderCompletedAt < endDate)
            .SumAsync(o => o.NetAmountCents);
    }

    public async Task<long> GetRevenueByDateRangeAsync(Guid shopId, DateTime startDate, DateTime endDate)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCompletedAt >= startDate &&
                   o.OrderCompletedAt < endDate)
            .SumAsync(o => o.NetAmountCents);
    }

    public async Task<long> GetNetRevenueByDateRangeAsync(Guid shopId, DateTime startDate, DateTime endDate)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCompletedAt >= startDate &&
                   o.OrderCompletedAt < endDate)
            .SumAsync(o => o.NetAmountCents);
    }

    public async Task<int> GetPageViewsAsync(Guid shopId, DateTime date)
    {
        // TODO: Cần integrate với Analytics service để lấy page views
        // Tạm thời return 0
        return await Task.FromResult(0);
    }

    public async Task<int> GetOrdersAwaitingActionCountAsync(Guid shopId)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   (o.Status == "PENDING" || 
                    o.Status == "CONFIRMED" || 
                    o.Status == "PROCESSING" ||
                    o.Status == "READY_TO_SHIP"))
            .CountAsync();
    }

    public async Task<decimal> GetConversionRateAsync(Guid shopId, DateTime date)
    {
        var orders = await GetRevenueByDateAsync(shopId, date);
        var pageViews = await GetPageViewsAsync(shopId, date);
        
        if (pageViews == 0) return 0;
        
        return (decimal)orders / pageViews * 100;
    }

    // ============================================
    // Time Series Data - Returns Entities
    // ============================================

    public async Task<List<OrderMetricsSnapshot>> GetRevenueLastNDaysAsync(Guid shopId, int days = 8)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var endDate = DateTime.UtcNow.Date.AddDays(1);

        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCompletedAt >= startDate &&
                   o.OrderCompletedAt < endDate)
            .OrderBy(o => o.OrderCompletedAt)
            .ToListAsync();
    }

    public async Task<List<OrderMetricsSnapshot>> GetRevenueGroupedByDateAsync(
        Guid shopId, DateTime startDate, DateTime endDate, string groupBy = "daily")
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCompletedAt >= startDate &&
                   o.OrderCompletedAt < endDate)
            .OrderBy(o => o.OrderCompletedAt)
            .ToListAsync();
    }

    // ============================================
    // Top Products
    // ============================================

    public async Task<List<OrderMetricsSnapshot>> GetTopProductsByShopAsync(
        Guid shopId, DateTime periodStart, DateTime periodEnd, int limit = 10)
    {
        return await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderCreatedAt >= periodStart &&
                   o.OrderCreatedAt < periodEnd &&
                   o.ProductVersionId.HasValue &&
                   !string.IsNullOrEmpty(o.ProductVersionName))
            .GroupBy(o => o.ProductVersionId)
            .Select(g => g.OrderByDescending(x => x.NetAmountCents).First())
            .OrderByDescending(x => x.NetAmountCents)
            .Take(limit)
            .ToListAsync();
    }

    // ============================================
    // Monthly/Quarterly/Yearly Analytics - Returns Tuples
    // ============================================

    public async Task<List<(int Month, string MonthName, long Revenue, int OrderCount, int CompletedCount, decimal AvgOrderValue)>> 
        GetMonthlyRevenueAsync(Guid shopId, int year)
    {
        var monthNames = new[] { "", "January", "February", "March", "April", "May", "June", 
                                 "July", "August", "September", "October", "November", "December" };

        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderYear == year)
            .GroupBy(o => o.OrderMonth)
            .Select(g => new
            {
                Month = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrdersCount = g.Count(),
                CompletedCount = g.Count(o => o.OrderCompletedAt.HasValue),
                AvgOrderValue = g.Average(o => o.NetAmountCents)
            })
            .OrderBy(x => x.Month)
            .ToListAsync();

        return data.Select(item => (
            item.Month,
            monthNames[item.Month],
            item.Revenue,
            item.OrdersCount,
            item.CompletedCount,
            (decimal)item.AvgOrderValue
        )).ToList();
    }

    public async Task<List<(int Quarter, string QuarterName, long Revenue, int OrderCount, decimal AvgMonthly)>> 
        GetQuarterlyRevenueAsync(Guid shopId, int year)
    {
        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderYear == year)
            .ToListAsync();

        var grouped = data
            .GroupBy(o => (o.OrderMonth - 1) / 3 + 1)
            .Select(g => new
            {
                Quarter = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrdersCount = g.Count(),
                AvgMonthlyRevenue = g.Sum(o => o.NetAmountCents) / 3m
            })
            .OrderBy(x => x.Quarter)
            .ToList();

        return grouped.Select(item => (
            item.Quarter,
            $"Q{item.Quarter}",
            item.Revenue,
            item.OrdersCount,
            item.AvgMonthlyRevenue
        )).ToList();
    }

    public async Task<List<(int Year, long Revenue, int OrderCount, decimal AvgMonthly)>> 
        GetYearlyRevenueAsync(Guid shopId, int startYear, int endYear)
    {
        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.Status == "COMPLETED" &&
                   o.OrderYear >= startYear &&
                   o.OrderYear <= endYear)
            .GroupBy(o => o.OrderYear)
            .Select(g => new
            {
                Year = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrdersCount = g.Count(),
                AvgMonthlyRevenue = g.Sum(o => o.NetAmountCents) / 12
            })
            .OrderBy(x => x.Year)
            .ToListAsync();

        return data.Select(item => (
            item.Year,
            item.Revenue,
            item.OrdersCount,
            (decimal)item.AvgMonthlyRevenue
        )).ToList();
    }

    // ============================================
    // Status/Breakdown Queries
    // ============================================

    public async Task<Dictionary<string, int>> GetOrdersCountByStatusAsync(
        Guid shopId, DateTime startDate, DateTime endDate)
    {
        var result = new Dictionary<string, int>();

        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in data)
        {
            result[item.Status] = item.Count;
        }

        return result;
    }

    public async Task<int> GetOrdersCountByCategoryAsync(Guid shopId, Guid categoryId, DateTime startDate, DateTime endDate)
    {
        return await Task.FromResult(0);
    }

    public async Task<List<(string Category, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByCategoryAsync(Guid shopId, DateTime startDate, DateTime endDate)
    {
        var totalRevenue = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate &&
                   !string.IsNullOrEmpty(o.CategoryName))
            .SumAsync(o => o.NetAmountCents);

        if (totalRevenue == 0)
            return new List<(string, long, int, decimal)>();

        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate &&
                   !string.IsNullOrEmpty(o.CategoryName))
            .GroupBy(o => o.CategoryName!)
            .Select(g => new
            {
                Category = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        return data.Select(item => (
            item.Category,
            item.Revenue,
            item.OrderCount,
            ((decimal)item.Revenue / totalRevenue * 100)
        )).ToList();
    }

    public async Task<List<(string ProductName, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByProductAsync(Guid shopId, DateTime startDate, DateTime endDate, int limit = 50)
    {
        var totalRevenue = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate &&
                   !string.IsNullOrEmpty(o.ProductVersionName))
            .SumAsync(o => o.NetAmountCents);

        if (totalRevenue == 0)
            return new List<(string, long, int, decimal)>();

        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate &&
                   !string.IsNullOrEmpty(o.ProductVersionName))
            .GroupBy(o => o.ProductVersionName!)
            .Select(g => new
            {
                ProductName = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync();

        return data.Select(item => (
            item.ProductName,
            item.Revenue,
            item.OrderCount,
            ((decimal)item.Revenue / totalRevenue * 100)
        )).ToList();
    }

    public async Task<List<(string Status, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByOrderStatusAsync(Guid shopId, DateTime startDate, DateTime endDate)
    {
        var totalRevenue = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate)
            .SumAsync(o => o.NetAmountCents);

        if (totalRevenue == 0)
            return new List<(string, long, int, decimal)>();

        var data = await _context.OrderMetricsSnapshots
            .Where(o => o.ShopId == shopId && 
                   o.OrderCreatedAt >= startDate &&
                   o.OrderCreatedAt < endDate)
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Revenue = g.Sum(o => o.NetAmountCents),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync();

        return data.Select(item => (
            item.Status,
            item.Revenue,
            item.OrderCount,
            ((decimal)item.Revenue / totalRevenue * 100)
        )).ToList();
    }

    // ============================================
    // Cache Management
    // ============================================

    public async Task<string?> GetCachedMetricsAsync(Guid shopId)
    {
        try
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{shopId}";
            return await _cache.GetStringAsync(cacheKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SetCachedMetricsAsync(Guid shopId, string jsonData)
    {
        try
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{shopId}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_DURATION_SECONDS)
            };
            
            await _cache.SetStringAsync(cacheKey, jsonData, cacheOptions);
        }
        catch (Exception)
        {
            // Cache failure - không block operations
        }
    }

    public async Task InvalidateCacheAsync(Guid shopId)
    {
        try
        {
            var cacheKey = $"{CACHE_KEY_PREFIX}{shopId}";
            await _cache.RemoveAsync(cacheKey);
        }
        catch (Exception)
        {
        }
    }
}
