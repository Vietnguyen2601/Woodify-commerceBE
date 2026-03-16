using Microsoft.AspNetCore.Mvc;
using ShipmentService.Infrastructure.Cache;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IShopInfoCacheRepository _shopCache;
    private readonly IOrderInfoCacheRepository _orderCache;

    public TestController(
        IShopInfoCacheRepository shopCache,
        IOrderInfoCacheRepository orderCache)
    {
        _shopCache = shopCache;
        _orderCache = orderCache;
    }

    /// <summary>
    /// Seed a test shop into cache (for testing only)
    /// </summary>
    [HttpPost("seed-shop")]
    public async Task<IActionResult> SeedTestShop([FromBody] SeedShopRequest request)
    {
        var shopInfo = new ShopInfoCache
        {
            ShopId = request.ShopId,
            ShopName = request.ShopName ?? "Test Shop",
            DefaultPickupAddress = request.DefaultPickupAddress ?? "1442",
            DefaultProviderServiceCode = request.DefaultProviderServiceCode ?? "STD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        await _shopCache.SaveShopInfoAsync(shopInfo);

        return Ok(new
        {
            message = "Test shop seeded successfully",
            data = shopInfo
        });
    }

    /// <summary>
    /// Seed a test order into cache (for testing only)
    /// </summary>
    [HttpPost("seed-order")]
    public async Task<IActionResult> SeedTestOrder([FromBody] SeedOrderRequest request)
    {
        var orderInfo = new OrderInfoCache
        {
            OrderId = request.OrderId,
            ShopId = request.ShopId,
            AccountId = request.AccountId,
            DeliveryAddress = request.DeliveryAddress ?? "1444_21105",
            TotalAmountCents = request.TotalAmountCents,
            CreatedAt = DateTime.UtcNow
        };

        await _orderCache.SaveOrderInfoAsync(orderInfo);

        return Ok(new
        {
            message = "Test order seeded successfully",
            data = orderInfo
        });
    }

    /// <summary>
    /// Get shop from cache by ID
    /// </summary>
    [HttpGet("shop/{shopId}")]
    public async Task<IActionResult> GetShop(Guid shopId)
    {
        var shop = await _shopCache.GetShopInfoAsync(shopId);
        if (shop == null)
            return NotFound(new { message = "Shop not found in cache" });

        return Ok(shop);
    }

    /// <summary>
    /// Get order from cache by ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var order = await _orderCache.GetOrderInfoAsync(orderId);
        if (order == null)
            return NotFound(new { message = "Order not found in cache" });

        return Ok(order);
    }
}

public class SeedShopRequest
{
    public Guid ShopId { get; set; }
    public string? ShopName { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public string? DefaultProviderServiceCode { get; set; }
}

public class SeedOrderRequest
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string? DeliveryAddress { get; set; }
    public double TotalAmountCents { get; set; }
}
