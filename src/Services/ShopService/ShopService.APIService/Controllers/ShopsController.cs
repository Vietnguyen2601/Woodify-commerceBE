using Microsoft.AspNetCore.Mvc;
using ShopService.Common.DTOs;
using ShopService.Services.Interfaces;

namespace ShopService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopsController(IShopService shopService)
    {
        _shopService = shopService;
    }

    /// <summary>
    /// Lấy danh sách tất cả shops
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShopDto>>> GetAllShops()
    {
        var shops = await _shopService.GetAllShopsAsync();
        return Ok(shops);
    }

    /// <summary>
    /// Lấy shop theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ShopDto>> GetShopById(Guid id)
    {
        var shop = await _shopService.GetShopByIdAsync(id);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    /// <summary>
    /// Lấy shop theo Owner ID
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<ShopDto>> GetShopByOwnerId(Guid ownerId)
    {
        var shop = await _shopService.GetShopByOwnerIdAsync(ownerId);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    /// <summary>
    /// Tạo shop mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ShopDto>> CreateShop([FromBody] CreateShopDto dto)
    {
        var shop = await _shopService.CreateShopAsync(dto);
        return CreatedAtAction(nameof(GetShopById), new { id = shop.ShopId }, shop);
    }

    /// <summary>
    /// Cập nhật shop
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ShopDto>> UpdateShop(Guid id, [FromBody] UpdateShopDto dto)
    {
        var shop = await _shopService.UpdateShopAsync(id, dto);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    /// <summary>
    /// Xóa shop (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShop(Guid id)
    {
        var result = await _shopService.DeleteShopAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
