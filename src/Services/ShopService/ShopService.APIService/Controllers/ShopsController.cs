using Microsoft.AspNetCore.Mvc;
using Shared.Results;
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
    public async Task<ActionResult<ServiceResult<IEnumerable<ShopDto>>>> GetAllShops()
    {
        var result = await _shopService.GetAllShopsAsync();
        return Ok(result);
    }

    [HttpGet("GetShopById/{id}")]
    public async Task<ActionResult<ServiceResult<ShopDto>>> GetShopById(Guid id)
    {
        var result = await _shopService.GetShopByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetShopByOwnerId/{ownerId}")]
    public async Task<ActionResult<ServiceResult<ShopDto>>> GetShopByOwnerId(Guid ownerId)
    {
        var result = await _shopService.GetShopByOwnerIdAsync(ownerId);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpPost("CreateShop")]
    public async Task<ActionResult<ServiceResult<ShopDto>>> CreateShop([FromBody] CreateShopDto dto)
    {
        var result = await _shopService.CreateShopAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetShopById), new { id = result.Data?.ShopId }, result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateShop/{id}")]
    public async Task<ActionResult<ServiceResult<ShopDto>>> UpdateShop(Guid id, [FromBody] UpdateShopDto dto)
    {
        var result = await _shopService.UpdateShopAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("DeleteShop/{id}")]
    public async Task<ActionResult<ServiceResult>> DeleteShop(Guid id)
    {
        var result = await _shopService.DeleteShopAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }
}


