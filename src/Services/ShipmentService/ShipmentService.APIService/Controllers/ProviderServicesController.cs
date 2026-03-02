using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProviderServicesController : ControllerBase
{
    private readonly IProviderServiceService _providerServiceService;

    public ProviderServicesController(IProviderServiceService providerServiceService)
    {
        _providerServiceService = providerServiceService;
    }

    [HttpGet("GetAllServices")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetAll()
    {
        var result = await _providerServiceService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetServiceById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> GetById(Guid id)
    {
        var result = await _providerServiceService.GetByIdAsync(id);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("GetServicesByProvider/{providerId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetByProvider(Guid providerId)
    {
        var result = await _providerServiceService.GetByProviderIdAsync(providerId);
        return Ok(result);
    }

    [HttpPost("CreateService")]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> Create([FromBody] CreateProviderServiceDto dto)
    {
        var result = await _providerServiceService.CreateAsync(dto);
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.ServiceId }, result);
        return BadRequest(result);
    }

    [HttpPut("UpdateService/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> Update(Guid id, [FromBody] UpdateProviderServiceDto dto)
    {
        var result = await _providerServiceService.UpdateAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("DeleteService/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _providerServiceService.DeleteAsync(id);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }
}
