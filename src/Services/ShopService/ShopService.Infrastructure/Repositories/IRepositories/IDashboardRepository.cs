using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Repositories.IRepositories;

/// <summary>
/// Repository cho Dashboard metrics - read-only queries
/// Trả về Entities hoặc primitive types
/// Service layer sẽ map sang DTOs
/// </summary>
public interface IDashboardRepository
{
    // ============================================
    // Task Board Queries
    // ============================================
    
    Task<int> GetAwaitingPickupCountAsync(Guid shopId);
    Task<int> GetReadyToShipCountAsync(Guid shopId);
    Task<int> GetReturnsRefundsCountAsync(Guid shopId);
    Task<int> GetLockedProductsCountAsync(Guid shopId);
    Task<int> GetOrdersSLAViolationCountAsync(Guid shopId);

    // ============================================
    // KPI Queries
    // ============================================
    
    Task<long> GetRevenueByDateAsync(Guid shopId, DateTime date);
    Task<long> GetRevenueByDateRangeAsync(Guid shopId, DateTime startDate, DateTime endDate);
    Task<long> GetNetRevenueByDateRangeAsync(Guid shopId, DateTime startDate, DateTime endDate);
    Task<int> GetPageViewsAsync(Guid shopId, DateTime date);
    Task<int> GetOrdersAwaitingActionCountAsync(Guid shopId);
    Task<decimal> GetConversionRateAsync(Guid shopId, DateTime date);

    // ============================================
    // Time Series Data
    // ============================================
    
    Task<List<OrderMetricsSnapshot>> GetRevenueLastNDaysAsync(Guid shopId, int days = 8);
    Task<List<OrderMetricsSnapshot>> GetRevenueGroupedByDateAsync(
        Guid shopId, DateTime startDate, DateTime endDate, string groupBy = "daily");

    // ============================================
    // Top Products/Categories
    // ============================================
    
    Task<List<OrderMetricsSnapshot>> GetTopProductsByShopAsync(
        Guid shopId, DateTime periodStart, DateTime periodEnd, int limit = 10);

    // ============================================
    // Monthly/Quarterly/Yearly Analytics
    // ============================================
    
    Task<List<(int Month, string MonthName, long Revenue, int OrderCount, int CompletedCount, decimal AvgOrderValue)>> 
        GetMonthlyRevenueAsync(Guid shopId, int year);

    Task<List<(int Quarter, string QuarterName, long Revenue, int OrderCount, decimal AvgMonthly)>> 
        GetQuarterlyRevenueAsync(Guid shopId, int year);

    Task<List<(int Year, long Revenue, int OrderCount, decimal AvgMonthly)>> 
        GetYearlyRevenueAsync(Guid shopId, int startYear, int endYear);

    // ============================================
    // Status/Breakdown Queries
    // ============================================
    
    Task<Dictionary<string, int>> GetOrdersCountByStatusAsync(
        Guid shopId, DateTime startDate, DateTime endDate);

    Task<int> GetOrdersCountByCategoryAsync(Guid shopId, Guid categoryId, DateTime startDate, DateTime endDate);

    Task<List<(string Category, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByCategoryAsync(Guid shopId, DateTime startDate, DateTime endDate);

    Task<List<(string ProductName, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByProductAsync(Guid shopId, DateTime startDate, DateTime endDate, int limit = 50);

    Task<List<(string Status, long Revenue, int OrderCount, decimal Percentage)>> 
        GetRevenueBreakdownByOrderStatusAsync(Guid shopId, DateTime startDate, DateTime endDate);

    // ============================================
    // Cache Management
    // ============================================
    
    Task<string?> GetCachedMetricsAsync(Guid shopId);
    Task SetCachedMetricsAsync(Guid shopId, string jsonData);
    Task InvalidateCacheAsync(Guid shopId);
}
