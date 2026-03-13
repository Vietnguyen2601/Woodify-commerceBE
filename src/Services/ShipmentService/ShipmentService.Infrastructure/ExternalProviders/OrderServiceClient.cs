using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ShipmentService.Infrastructure.ExternalProviders;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<OrderServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrderServiceClient(HttpClient http, ILogger<OrderServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<OrderItemInfo>?> GetOrderItemsAsync(Guid orderId)
    {
        var ctx = await GetOrderContextAsync(orderId);
        return ctx?.Items;
    }

    public async Task<OrderContext?> GetOrderContextAsync(Guid orderId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/Orders/{orderId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OrderService returned {Status} for order {OrderId}", response.StatusCode, orderId);
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync();
            using var wrapper = JsonDocument.Parse(raw);

            if (!wrapper.RootElement.TryGetProperty("data", out var data))
            {
                _logger.LogWarning("OrderService response for {OrderId} missing 'data' field", orderId);
                return null;
            }

            var ctx = new OrderContext();

            if (data.TryGetProperty("shopId", out var shopIdEl) &&
                Guid.TryParse(shopIdEl.GetString(), out var shopId))
                ctx.ShopId = shopId;

            if (data.TryGetProperty("deliveryAddressId", out var addrEl))
                ctx.DeliveryAddressId = addrEl.GetString();

            if (data.TryGetProperty("orderItems", out var itemsEl))
                ctx.Items = JsonSerializer.Deserialize<List<OrderItemInfo>>(itemsEl.GetRawText(), JsonOptions) ?? new();

            return ctx;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OrderService for order {OrderId}", orderId);
            return null;
        }
    }
}
