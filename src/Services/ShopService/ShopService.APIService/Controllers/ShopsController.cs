using Microsoft.AspNetCore.Mvc;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;

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

    [HttpGet("GetAllShops")]
    public async Task<ActionResult<IEnumerable<ShopDto>>> GetAllShops()
    {
        var shops = await _shopService.GetAllShopsAsync();
        return Ok(shops);
    }

    [HttpGet("GetShopById/{id}")]
    public async Task<ActionResult<ShopDto>> GetShopById(Guid id)
    {
        var shop = await _shopService.GetShopByIdAsync(id);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    [HttpGet("GetShopByOwnerId/{ownerId}")]
    public async Task<ActionResult<ShopDto>> GetShopByOwnerId(Guid ownerId)
    {
        var shop = await _shopService.GetShopByOwnerIdAsync(ownerId);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    [HttpPost("CreateShop")]
    public async Task<ActionResult<ShopDto>> CreateShop([FromBody] CreateShopDto dto)
    {
        var shop = await _shopService.CreateShopAsync(dto);
        return CreatedAtAction(nameof(GetShopById), new { id = shop.ShopId }, shop);
    }

    [HttpPut("UpdateShop/{id}")]
    public async Task<ActionResult<ShopDto>> UpdateShop(Guid id, [FromBody] UpdateShopDto dto)
    {
        var shop = await _shopService.UpdateShopAsync(id, dto);
        if (shop == null) return NotFound();
        return Ok(shop);
    }

    [HttpDelete("DeleteShop/{id}")]
    public async Task<IActionResult> DeleteShop(Guid id)
    {
        var result = await _shopService.DeleteShopAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
