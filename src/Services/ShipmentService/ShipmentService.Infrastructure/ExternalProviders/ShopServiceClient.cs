using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ShipmentService.Infrastructure.ExternalProviders;

public class ShopServiceClient : IShopServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ShopServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ShopServiceClient(HttpClient http, ILogger<ShopServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string?> GetDefaultPickupAddressAsync(Guid shopId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/Shops/GetShopById/{shopId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ShopService returned {Status} for shop {ShopId}", response.StatusCode, shopId);
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync();
            using var wrapper = JsonDocument.Parse(raw);

            if (!wrapper.RootElement.TryGetProperty("data", out var data))
            {
                _logger.LogWarning("ShopService response for {ShopId} missing 'data' field", shopId);
                return null;
            }

            if (data.TryGetProperty("defaultPickupAddress", out var addrEl))
                return addrEl.GetString();

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ShopService for shop {ShopId}", shopId);
            return null;
        }
    }
}
