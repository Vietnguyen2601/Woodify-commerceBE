using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;

namespace ShopService.APIService.Controllers;

[ApiController]
[Route("api/shop/[controller]")]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopsController(IShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet("GetAllShops")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShopPublicDto>>>> GetAllShops()
    {
        var result = await _shopService.GetAllShopsAsync();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/GetAllShops")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShopDto>>>> GetAllShopsAdmin()
    {
        var result = await _shopService.GetAllShopsAdminAsync();
        return Ok(result);
    }

    [HttpGet("GetShopById/{id}")]
    public async Task<ActionResult<ServiceResult<ShopPublicDto>>> GetShopById(Guid id)
    {
        var result = await _shopService.GetShopByIdAsync(id);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("GetShopByOwnerId/{ownerId}")]
    public async Task<ActionResult<ServiceResult<ShopDetailDto>>> GetShopByOwnerId(Guid ownerId)
    {
        var result = await _shopService.GetShopByOwnerIdAsync(ownerId);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPatch("UpdateShopInfo/{id}")]
    public async Task<ActionResult<ServiceResult<UpdateShopInfoResponseDto>>> UpdateShopInfo(Guid id, [FromBody] UpdateShopInfoDto dto)
    {
        var result = await _shopService.UpdateShopInfoAsync(id, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status != 200)
            return StatusCode(result.Status, result);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<RegisterShopResponseDto>>> RegisterShop([FromBody] RegisterShopDto dto)
    {
        var result = await _shopService.RegisterShopAsync(dto);

        if (result.Status == 201)
            return CreatedAtAction(nameof(GetShopById), new { id = result.Data?.ShopId }, result);

        return StatusCode(result.Status, result);
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
            return StatusCode(result.Status, result);

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

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ServiceResult<UpdateShopStatusResponseDto>>> UpdateShopStatus(Guid id, [FromBody] UpdateShopStatusDto dto)
    {
        var result = await _shopService.UpdateShopStatusAsync(id, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status != 200)
            return BadRequest(result);

        return Ok(result);
    }
}


