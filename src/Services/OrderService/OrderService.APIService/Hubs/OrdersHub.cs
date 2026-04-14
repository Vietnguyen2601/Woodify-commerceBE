using Microsoft.AspNetCore.SignalR;

namespace OrderService.APIService.Hubs;

/// <summary>
/// Real-time order + shipment status. Clients join groups then receive <c>OrderShipmentStatusUpdated</c>.
/// Hub path: <c>/hubs/orders</c>
/// </summary>
public class OrdersHub : Hub
{
    public const string OrderShipmentStatusUpdated = "OrderShipmentStatusUpdated";

    private readonly ILogger<OrdersHub> _logger;

    public OrdersHub(ILogger<OrdersHub> logger)
    {
        _logger = logger;
    }

    /// <summary>Subscribe as buyer for one order.</summary>
    public async Task JoinOrderGroup(string orderId)
    {
        if (Guid.TryParse(orderId, out _))
            await Groups.AddToGroupAsync(Context.ConnectionId, OrderGroupName(orderId));
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        if (Guid.TryParse(orderId, out _))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, OrderGroupName(orderId));
    }

    /// <summary>Subscribe as seller for all updates tagged with this shop.</summary>
    public async Task JoinShopGroup(string shopId)
    {
        if (Guid.TryParse(shopId, out _))
            await Groups.AddToGroupAsync(Context.ConnectionId, ShopGroupName(shopId));
    }

    public async Task LeaveShopGroup(string shopId)
    {
        if (Guid.TryParse(shopId, out _))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ShopGroupName(shopId));
    }

    /// <summary>Subscribe as buyer account (all their orders).</summary>
    public async Task JoinAccountGroup(string accountId)
    {
        if (Guid.TryParse(accountId, out _))
            await Groups.AddToGroupAsync(Context.ConnectionId, AccountGroupName(accountId));
    }

    public static string OrderGroupName(string orderId) => $"order:{orderId}";
    public static string ShopGroupName(string shopId) => $"shop:{shopId}";
    public static string AccountGroupName(string accountId) => $"account:{accountId}";

    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("OrdersHub connected {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug(exception, "OrdersHub disconnected {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
