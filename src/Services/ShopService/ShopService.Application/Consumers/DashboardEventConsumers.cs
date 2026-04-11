using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Events;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Repositories.IRepositories;

namespace ShopService.Application.Consumers;

/// <summary>
/// Event consumer base class
/// Xử lý việc consuming OrderService events và update ShopService metrics
/// </summary>
public abstract class DashboardEventConsumerBase
{
    protected readonly IDashboardRepository _dashboardRepository;
    protected readonly IDashboardService _dashboardService;
    protected readonly IDistributedCache _cache;
    protected readonly ILogger _logger;
    protected readonly ShopDbContext _context;
    protected const string CACHE_KEY_PREFIX = "dashboard:";
    protected const int CACHE_DURATION_SECONDS = 30;

    protected DashboardEventConsumerBase(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger logger,
        ShopDbContext context)
    {
        _dashboardRepository = dashboardRepository;
        _dashboardService = dashboardService;
        _cache = cache;
        _logger = logger;
        _context = context;
    }

    protected async Task InvalidateCacheAsync(Guid shopId)
    {
        try
        {
            await _dashboardRepository.InvalidateCacheAsync(shopId);
            _logger.LogInformation("Cache invalidated for shop {ShopId}", shopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for shop {ShopId}", shopId);
        }
    }

    protected async Task RefreshMetricsAsync(Guid shopId)
    {
        try
        {
            await _dashboardService.RefreshMetricsCacheAsync(shopId);
            _logger.LogInformation("Metrics refreshed for shop {ShopId}", shopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing metrics for shop {ShopId}", shopId);
        }
    }

    protected async Task SaveOrderMetricsAsync(OrderMetricsSnapshot metrics)
    {
        try
        {
            var existing = await _context.OrderMetricsSnapshots
                .FirstOrDefaultAsync(o => o.OrderId == metrics.OrderId);

            if (existing != null)
            {
                existing.Status = metrics.Status;
                existing.TotalAmountCents = metrics.TotalAmountCents;
                existing.CommissionCents = metrics.CommissionCents;
                existing.NetAmountCents = metrics.NetAmountCents;
                existing.OrderCompletedAt = metrics.OrderCompletedAt;
                existing.RefundedAt = metrics.RefundedAt;
                existing.RefundAmountCents = metrics.RefundAmountCents;
                existing.RefundReason = metrics.RefundReason;
                existing.IsReturn = metrics.IsReturn;
                existing.IsSLAViolated = metrics.IsSLAViolated;
                existing.LastUpdatedAt = DateTime.UtcNow;
                _context.OrderMetricsSnapshots.Update(existing);
            }
            else
            {
                metrics.CreatedAt = DateTime.UtcNow;
                metrics.LastUpdatedAt = DateTime.UtcNow;
                _context.OrderMetricsSnapshots.Add(metrics);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order metrics for order {OrderId}", metrics.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderStatusChangedEvent
/// </summary>
public class OrderStatusChangedEventConsumer : DashboardEventConsumerBase
{
    public OrderStatusChangedEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderStatusChangedEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderStatusChangedEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing OrderStatusChangedEvent: OrderId={OrderId}, Shop={ShopId}, Status: {PreviousStatus} → {NewStatus}, ItemCount={ItemCount}",
                @event.OrderId, @event.ShopId, @event.PreviousStatus, @event.NewStatus, @event.ItemCount);

            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = @event.NewStatus,
                TotalAmountCents = @event.TotalAmountCents,
                OrderCreatedAt = @event.OrderCreatedAt,
                OrderYear = @event.OrderCreatedAt.Year,
                OrderMonth = @event.OrderCreatedAt.Month,
                OrderDay = @event.OrderCreatedAt.Day,
                IsSLAViolated = (DateTime.UtcNow - @event.OrderCreatedAt).TotalMinutes > 45 &&
                                (@event.NewStatus == "CONFIRMED" || @event.NewStatus == "PROCESSING" || @event.NewStatus == "READY_TO_SHIP"),
                ItemCount = @event.ItemCount,
                ProductVersionId = @event.ProductVersionId,
                ProductVersionName = @event.ProductVersionName,
                CategoryId = @event.CategoryId,
                CategoryName = @event.CategoryName
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderStatusChangedEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderCompletedEvent
/// </summary>
public class OrderCompletedEventConsumer : DashboardEventConsumerBase
{
    public OrderCompletedEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderCompletedEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderCompletedEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing OrderCompletedEvent: OrderId={OrderId}, Shop={ShopId}, Amount={Amount} VND, ItemCount={ItemCount}",
                @event.OrderId, @event.ShopId, @event.TotalAmountCents / 100m, @event.ItemCount);

            var now = DateTime.UtcNow;
            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = "COMPLETED",
                TotalAmountCents = @event.TotalAmountCents,
                CommissionCents = @event.CommissionCents,
                NetAmountCents = @event.TotalAmountCents - @event.CommissionCents,
                OrderCompletedAt = @event.CompletedAt,
                OrderCreatedAt = @event.CompletedAt.AddDays(-1),
                OrderYear = @event.CompletedAt.Year,
                OrderMonth = @event.CompletedAt.Month,
                OrderDay = @event.CompletedAt.Day,
                ItemCount = @event.ItemCount,
                ProductVersionId = @event.ProductVersionId,
                ProductVersionName = @event.ProductVersionName,
                CategoryId = @event.CategoryId,
                CategoryName = @event.CategoryName
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
            await RefreshMetricsAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCompletedEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderCancelledEvent
/// </summary>
public class OrderCancelledEventConsumer : DashboardEventConsumerBase
{
    public OrderCancelledEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderCancelledEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderCancelledEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing OrderCancelledEvent: OrderId={OrderId}, Shop={ShopId}, Reason={Reason}, ItemCount={ItemCount}",
                @event.OrderId, @event.ShopId, @event.CancelReason, @event.ItemCount);

            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = "CANCELLED",
                TotalAmountCents = @event.CancelledAmountCents,
                OrderCreatedAt = @event.CancelledAt.AddDays(-1),
                OrderYear = @event.CancelledAt.Year,
                OrderMonth = @event.CancelledAt.Month,
                OrderDay = @event.CancelledAt.Day,
                ItemCount = @event.ItemCount,
                ProductVersionId = @event.ProductVersionId,
                ProductVersionName = @event.ProductVersionName,
                CategoryId = @event.CategoryId,
                CategoryName = @event.CategoryName
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
            await RefreshMetricsAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCancelledEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderRefundedEvent
/// </summary>
public class OrderRefundedEventConsumer : DashboardEventConsumerBase
{
    public OrderRefundedEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderRefundedEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderRefundedEvent @event)
    {
        try
        {
            var refundDuration = @event.RefundedAt - @event.OrderCreatedAt;
            var isSLAViolated = refundDuration.TotalHours > 24;

            _logger.LogInformation(
                "Processing OrderRefundedEvent: OrderId={OrderId}, Shop={ShopId}, Amount={Amount} VND, ItemCount={ItemCount}",
                @event.OrderId, @event.ShopId, @event.RefundAmountCents / 100m, @event.ItemCount);

            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = "REFUNDED",
                RefundedAt = @event.RefundedAt,
                RefundAmountCents = @event.RefundAmountCents,
                RefundReason = @event.RefundReason,
                IsReturn = true,
                IsSLAViolated = isSLAViolated,
                OrderCreatedAt = @event.OrderCreatedAt,
                OrderYear = @event.OrderCreatedAt.Year,
                OrderMonth = @event.OrderCreatedAt.Month,
                OrderDay = @event.OrderCreatedAt.Day,
                ItemCount = @event.ItemCount,
                ProductVersionId = @event.ProductVersionId,
                ProductVersionName = @event.ProductVersionName,
                CategoryId = @event.CategoryId,
                CategoryName = @event.CategoryName
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
            await RefreshMetricsAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderRefundedEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderAwaitingPickupEvent
/// </summary>
public class OrderAwaitingPickupEventConsumer : DashboardEventConsumerBase
{
    public OrderAwaitingPickupEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderAwaitingPickupEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderAwaitingPickupEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing OrderAwaitingPickupEvent: OrderId={OrderId}, Shop={ShopId}",
                @event.OrderId, @event.ShopId);

            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = "CONFIRMED",
                OrderCreatedAt = @event.AwaitingPickupAt.AddMinutes(-10),
                OrderYear = @event.AwaitingPickupAt.Year,
                OrderMonth = @event.AwaitingPickupAt.Month,
                OrderDay = @event.AwaitingPickupAt.Day
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderAwaitingPickupEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho OrderReadyToShipEvent
/// </summary>
public class OrderReadyToShipEventConsumer : DashboardEventConsumerBase
{
    public OrderReadyToShipEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<OrderReadyToShipEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(OrderReadyToShipEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing OrderReadyToShipEvent: OrderId={OrderId}, Shop={ShopId}",
                @event.OrderId, @event.ShopId);

            var metrics = new OrderMetricsSnapshot
            {
                OrderId = @event.OrderId,
                ShopId = @event.ShopId,
                Status = "READY_TO_SHIP",
                OrderCreatedAt = @event.ReadyToShipAt.AddMinutes(-30),
                OrderYear = @event.ReadyToShipAt.Year,
                OrderMonth = @event.ReadyToShipAt.Month,
                OrderDay = @event.ReadyToShipAt.Day
            };

            await SaveOrderMetricsAsync(metrics);
            await InvalidateCacheAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderReadyToShipEvent for order {OrderId}", @event.OrderId);
        }
    }
}

/// <summary>
/// Event consumer cho MetricsAggregatedEvent (batch event)
/// </summary>
public class MetricsAggregatedEventConsumer : DashboardEventConsumerBase
{
    public MetricsAggregatedEventConsumer(
        IDashboardRepository dashboardRepository,
        IDashboardService dashboardService,
        IDistributedCache cache,
        ILogger<MetricsAggregatedEventConsumer> logger,
        ShopDbContext context)
        : base(dashboardRepository, dashboardService, cache, logger, context)
    {
    }

    public async Task HandleAsync(MetricsAggregatedEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Processing MetricsAggregatedEvent: Shop={ShopId}",
                @event.ShopId);

            await InvalidateCacheAsync(@event.ShopId);
            await RefreshMetricsAsync(@event.ShopId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MetricsAggregatedEvent for shop {ShopId}", @event.ShopId);
        }
    }
}
