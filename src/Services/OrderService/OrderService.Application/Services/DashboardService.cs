using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace OrderService.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IOrderRepository _orderRepository;

    public DashboardService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ServiceResult<RevenueAnalyticsResultDto>> GetQuarterlyRevenueAsync()
    {
        try
        {
            var result = await GetQuarterlyDataAsync();
            return ServiceResult<RevenueAnalyticsResultDto>.Success(
                result,
                "Quarterly revenue analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<RevenueAnalyticsResultDto>.InternalServerError(
                $"Error retrieving quarterly revenue analytics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<RevenueAnalyticsResultDto>> GetYearlyRevenueAsync()
    {
        try
        {
            var result = await GetYearlyDataAsync();
            return ServiceResult<RevenueAnalyticsResultDto>.Success(
                result,
                "Yearly revenue analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<RevenueAnalyticsResultDto>.InternalServerError(
                $"Error retrieving yearly revenue analytics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<RevenueAnalyticsResultDto>> GetCustomRevenueAsync(
        string startDate,
        string endDate,
        string? granularity = null)
    {
        try
        {
            // Validate dates
            if (!DateTime.TryParse(startDate, out var parsedStartDate) ||
                !DateTime.TryParse(endDate, out var parsedEndDate))
            {
                return ServiceResult<RevenueAnalyticsResultDto>.BadRequest(
                    "Invalid date format. Use yyyy-MM-dd");
            }

            if (parsedStartDate >= parsedEndDate)
            {
                return ServiceResult<RevenueAnalyticsResultDto>.BadRequest(
                    "Start date must be before end date");
            }

            // Auto-determine granularity if not specified
            if (string.IsNullOrWhiteSpace(granularity))
            {
                var dayDifference = (parsedEndDate - parsedStartDate).Days;
                granularity = dayDifference switch
                {
                    <= 31 => "DAILY",
                    <= 90 => "DAILY",
                    <= 365 => "MONTHLY",
                    _ => "MONTHLY"
                };
            }

            RevenueAnalyticsResultDto result;

            if (granularity.Equals("DAILY", StringComparison.OrdinalIgnoreCase))
            {
                result = await GetCustomDailyDataAsync(parsedStartDate, parsedEndDate);
            }
            else if (granularity.Equals("MONTHLY", StringComparison.OrdinalIgnoreCase))
            {
                result = await GetCustomMonthlyDataAsync(parsedStartDate, parsedEndDate);
            }
            else
            {
                result = await GetCustomDailyDataAsync(parsedStartDate, parsedEndDate);
            }

            return ServiceResult<RevenueAnalyticsResultDto>.Success(
                result,
                "Custom revenue analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<RevenueAnalyticsResultDto>.InternalServerError(
                $"Error retrieving custom revenue analytics: {ex.Message}");
        }
    }

    private async Task<RevenueAnalyticsResultDto> GetQuarterlyDataAsync()
    {
        var orders = await _orderRepository.GetAllAsync();

        var groupedData = orders
            .Where(o => o.Status == OrderStatus.COMPLETED && o.CreatedAt >= DateTime.UtcNow.AddYears(-1))
            .GroupBy(o => new
            {
                Year = o.CreatedAt.Year,
                Quarter = (o.CreatedAt.Month - 1) / 3 + 1
            })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Quarter)
            .ToList();

        var chartData = new List<RevenueDataPointDto>();
        RevenueDataPointDto? previousQuarter = null;

        foreach (var group in groupedData)
        {
            var dataPoint = new RevenueDataPointDto
            {
                Year = group.Key.Year,
                Quarter = group.Key.Quarter,
                Period = $"Q{group.Key.Quarter}-{group.Key.Year}",
                GrossRevenue = (long)group.Sum(o => o.TotalAmountCents),
                CommissionRevenue = group.Sum(o => o.CommissionCents),
                OrderCount = group.Count(),
                CommissionRate = (decimal)(group.Sum(o => o.CommissionRate) / group.Count()),
            };

            dataPoint.NetRevenue = dataPoint.GrossRevenue - dataPoint.CommissionRevenue;
            dataPoint.AverageOrderValue = dataPoint.OrderCount > 0
                ? dataPoint.GrossRevenue / dataPoint.OrderCount
                : 0;

            // Calculate QoQ Growth
            if (previousQuarter != null)
            {
                if (previousQuarter.GrossRevenue > 0)
                {
                    dataPoint.QoQGrowth = (decimal)(
                        (dataPoint.GrossRevenue - previousQuarter.GrossRevenue) /
                        (double)previousQuarter.GrossRevenue * 100);
                }
            }

            // Calculate YoY Growth (compare with same quarter last year)
            var lastYearSameQuarter = groupedData.FirstOrDefault(g =>
                g.Key.Year == group.Key.Year - 1 && g.Key.Quarter == group.Key.Quarter);

            if (lastYearSameQuarter != null)
            {
                var lastYearRevenue = (long)lastYearSameQuarter.Sum(o => o.TotalAmountCents);
                if (lastYearRevenue > 0)
                {
                    dataPoint.YoYGrowth = (decimal)(
                        (dataPoint.GrossRevenue - lastYearRevenue) /
                        (double)lastYearRevenue * 100);
                }
            }

            chartData.Add(dataPoint);
            previousQuarter = dataPoint;
        }

        var summary = CalculateSummary(chartData, "QUARTERLY");

        var result = new RevenueAnalyticsResultDto
        {
            Data = new RevenueAnalyticsDataDto
            {
                TimeRange = "LAST_4_QUARTERS",
                Granularity = "QUARTERLY",
                Currency = "VND",
                GeneratedAt = DateTime.UtcNow,
                CacheStatus = "HIT",
                CacheTTL = 86400,
                Summary = summary,
                ChartData = chartData
            }
        };

        return result;
    }

    private async Task<RevenueAnalyticsResultDto> GetYearlyDataAsync()
    {
        var orders = await _orderRepository.GetAllAsync();

        var groupedData = orders
            .Where(o => o.Status == OrderStatus.COMPLETED && o.CreatedAt >= DateTime.UtcNow.AddYears(-3))
            .GroupBy(o => o.CreatedAt.Year)
            .OrderByDescending(g => g.Key)
            .ToList();

        var chartData = new List<RevenueDataPointDto>();
        RevenueDataPointDto? previousYear = null;

        foreach (var group in groupedData)
        {
            var dataPoint = new RevenueDataPointDto
            {
                Year = group.Key,
                GrossRevenue = (long)group.Sum(o => o.TotalAmountCents),
                CommissionRevenue = group.Sum(o => o.CommissionCents),
                OrderCount = group.Count(),
                CommissionRate = (decimal)(group.Sum(o => o.CommissionRate) / group.Count()),
            };

            dataPoint.NetRevenue = dataPoint.GrossRevenue - dataPoint.CommissionRevenue;
            dataPoint.AverageOrderValue = dataPoint.OrderCount > 0
                ? dataPoint.GrossRevenue / dataPoint.OrderCount
                : 0;

            // Calculate YoY Growth
            if (previousYear != null && previousYear.GrossRevenue > 0)
            {
                dataPoint.YoYGrowth = (decimal)(
                    (dataPoint.GrossRevenue - previousYear.GrossRevenue) /
                    (double)previousYear.GrossRevenue * 100);
            }

            // Check if current year and mark as projected
            if (group.Key == DateTime.UtcNow.Year)
            {
                dataPoint.IsProjected = true;
                dataPoint.ProjectionNote = "Current year (Jan-Apr only, extrapolated)";
            }

            chartData.Add(dataPoint);
            previousYear = dataPoint;
        }

        var summary = CalculateSummary(chartData, "YEARLY");

        var result = new RevenueAnalyticsResultDto
        {
            Data = new RevenueAnalyticsDataDto
            {
                TimeRange = "LAST_3_YEARS",
                Granularity = "YEARLY",
                Currency = "VND",
                GeneratedAt = DateTime.UtcNow,
                CacheStatus = "HIT",
                CacheTTL = 172800,
                Summary = summary,
                ChartData = chartData
            }
        };

        return result;
    }

    private async Task<RevenueAnalyticsResultDto> GetCustomDailyDataAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetAllAsync();

        var filteredOrders = orders
            .Where(o => o.Status == OrderStatus.COMPLETED && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToList();

        var chartData = GetDailyData(filteredOrders, startDate, endDate);
        var summary = CalculateSummary(chartData, "DAILY");
        var daysDifference = (endDate - startDate).Days;

        var result = new RevenueAnalyticsResultDto
        {
            Data = new RevenueAnalyticsDataDto
            {
                TimeRange = "CUSTOM",
                Granularity = "DAILY",
                Currency = "VND",
                GeneratedAt = DateTime.UtcNow,
                CacheStatus = "MISS",
                CacheTTL = null,
                CustomRange = new CustomRangeDto
                {
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    Days = daysDifference
                },
                Summary = summary,
                ChartData = chartData
            }
        };

        return result;
    }

    private async Task<RevenueAnalyticsResultDto> GetCustomMonthlyDataAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetAllAsync();

        var filteredOrders = orders
            .Where(o => o.Status == OrderStatus.COMPLETED && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToList();

        var chartData = GetMonthlyData(filteredOrders, startDate, endDate);
        var summary = CalculateSummary(chartData, "MONTHLY");
        var daysDifference = (endDate - startDate).Days;

        var result = new RevenueAnalyticsResultDto
        {
            Data = new RevenueAnalyticsDataDto
            {
                TimeRange = "CUSTOM",
                Granularity = "MONTHLY",
                Currency = "VND",
                GeneratedAt = DateTime.UtcNow,
                CacheStatus = "MISS",
                CacheTTL = null,
                CustomRange = new CustomRangeDto
                {
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    Days = daysDifference
                },
                Summary = summary,
                ChartData = chartData
            }
        };

        return result;
    }

    private List<RevenueDataPointDto> GetDailyData(
        List<Order> orders,
        DateTime startDate,
        DateTime endDate)
    {
        var chartData = new List<RevenueDataPointDto>();

        var groupedByDay = orders
            .GroupBy(o => o.CreatedAt.Date)
            .OrderByDescending(g => g.Key)
            .ToList();

        RevenueDataPointDto? previousDay = null;

        foreach (var group in groupedByDay)
        {
            var dataPoint = new RevenueDataPointDto
            {
                Date = group.Key.ToString("yyyy-MM-dd"),
                GrossRevenue = (long)group.Sum(o => o.TotalAmountCents),
                CommissionRevenue = group.Sum(o => o.CommissionCents),
                OrderCount = group.Count(),
                CommissionRate = group.Count() > 0
                    ? (decimal)(group.Sum(o => o.CommissionRate) / group.Count())
                    : 0
            };

            dataPoint.NetRevenue = dataPoint.GrossRevenue - dataPoint.CommissionRevenue;
            dataPoint.AverageOrderValue = dataPoint.OrderCount > 0
                ? dataPoint.GrossRevenue / dataPoint.OrderCount
                : 0;

            // Calculate growth rate
            if (previousDay != null && previousDay.GrossRevenue > 0)
            {
                dataPoint.GrowthRate = (decimal)(
                    (dataPoint.GrossRevenue - previousDay.GrossRevenue) /
                    (double)previousDay.GrossRevenue * 100);
            }

            chartData.Add(dataPoint);
            previousDay = dataPoint;
        }

        return chartData;
    }

    private List<RevenueDataPointDto> GetMonthlyData(
        List<Order> orders,
        DateTime startDate,
        DateTime endDate)
    {
        var chartData = new List<RevenueDataPointDto>();

        var groupedByMonth = orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderByDescending(g => new { g.Key.Year, g.Key.Month })
            .ToList();

        RevenueDataPointDto? previousMonth = null;

        foreach (var group in groupedByMonth)
        {
            var dataPoint = new RevenueDataPointDto
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Period = $"{group.Key.Year:0000}-{group.Key.Month:00}",
                GrossRevenue = (long)group.Sum(o => o.TotalAmountCents),
                CommissionRevenue = group.Sum(o => o.CommissionCents),
                OrderCount = group.Count(),
                CommissionRate = group.Count() > 0
                    ? (decimal)(group.Sum(o => o.CommissionRate) / group.Count())
                    : 0
            };

            dataPoint.NetRevenue = dataPoint.GrossRevenue - dataPoint.CommissionRevenue;
            dataPoint.AverageOrderValue = dataPoint.OrderCount > 0
                ? dataPoint.GrossRevenue / dataPoint.OrderCount
                : 0;

            // Calculate growth rate
            if (previousMonth != null && previousMonth.GrossRevenue > 0)
            {
                dataPoint.GrowthRate = (decimal)(
                    (dataPoint.GrossRevenue - previousMonth.GrossRevenue) /
                    (double)previousMonth.GrossRevenue * 100);
            }

            chartData.Add(dataPoint);
            previousMonth = dataPoint;
        }

        return chartData;
    }

    private RevenueSummaryDto CalculateSummary(
        List<RevenueDataPointDto> chartData,
        string granularity)
    {
        if (chartData.Count == 0)
        {
            return new RevenueSummaryDto();
        }

        var totalGrossRevenue = chartData.Sum(d => d.GrossRevenue);
        var totalCommissionRevenue = chartData.Sum(d => d.CommissionRevenue);
        var totalNetRevenue = chartData.Sum(d => d.NetRevenue);
        var totalOrders = chartData.Sum(d => d.OrderCount);

        var summary = new RevenueSummaryDto
        {
            TotalGrossRevenue = totalGrossRevenue,
            TotalCommissionRevenue = totalCommissionRevenue,
            TotalNetRevenue = totalNetRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0 ? totalGrossRevenue / totalOrders : 0,
            AverageCommissionRate = chartData.Count > 0
                ? chartData.Average(d => d.CommissionRate)
                : 0
        };

        // Calculate average by granularity
        if (granularity == "DAILY")
        {
            summary.AverageDailyRevenue = chartData.Count > 0
                ? (long)(chartData.Sum(d => d.GrossRevenue) / chartData.Count)
                : 0;
        }
        else if (granularity == "MONTHLY")
        {
            summary.AverageMonthlyRevenue = chartData.Count > 0
                ? (long)(chartData.Sum(d => d.GrossRevenue) / chartData.Count)
                : 0;
        }
        else if (granularity == "QUARTERLY")
        {
            summary.AverageQuarterlyRevenue = chartData.Count > 0
                ? (long)(chartData.Sum(d => d.GrossRevenue) / chartData.Count)
                : 0;
        }
        else if (granularity == "YEARLY")
        {
            summary.AverageYearlyRevenue = chartData.Count > 0
                ? (long)(chartData.Sum(d => d.GrossRevenue) / chartData.Count)
                : 0;
        }

        // Calculate growth rate (first vs last)
        if (chartData.Count >= 2)
        {
            var firstRevenue = chartData[chartData.Count - 1].GrossRevenue;
            var lastRevenue = chartData[0].GrossRevenue;

            if (firstRevenue > 0)
            {
                summary.GrowthRate = (decimal)(
                    (lastRevenue - firstRevenue) / (double)firstRevenue * 100);
            }
        }

        // Find best and worst periods
        var bestPeriod = chartData.OrderByDescending(d => d.GrossRevenue).FirstOrDefault();
        var worstPeriod = chartData.OrderBy(d => d.GrossRevenue).FirstOrDefault();

        summary.BestPeriod = GetPeriodLabel(bestPeriod, granularity);
        summary.WorstPeriod = GetPeriodLabel(worstPeriod, granularity);

        return summary;
    }

    private string GetPeriodLabel(RevenueDataPointDto? dataPoint, string granularity)
    {
        if (dataPoint == null)
            return string.Empty;

        return granularity switch
        {
            "DAILY" => dataPoint.Date ?? string.Empty,
            "MONTHLY" => dataPoint.Period ?? string.Empty,
            "QUARTERLY" => dataPoint.Period ?? string.Empty,
            "YEARLY" => dataPoint.Year?.ToString() ?? string.Empty,
            _ => string.Empty
        };
    }
}
