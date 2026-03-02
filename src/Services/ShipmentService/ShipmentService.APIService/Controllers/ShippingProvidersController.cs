using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingProvidersController : ControllerBase
{
    private readonly IShippingProviderService _providerService;

    public ShippingProvidersController(IShippingProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet("GetAllProviders")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShippingProviderDto>>>> GetAll()
    {
        var result = await _providerService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetProviderById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> GetById(Guid id)
    {
        var result = await _providerService.GetByIdAsync(id);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("CreateProvider")]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Create([FromBody] CreateShippingProviderDto dto)
    {
        var result = await _providerService.CreateAsync(dto);
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.ProviderId }, result);
        return BadRequest(result);
    }

    [HttpPut("UpdateProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Update(Guid id, [FromBody] UpdateShippingProviderDto dto)
    {
        var result = await _providerService.UpdateAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("DeleteProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _providerService.DeleteAsync(id);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }
}
