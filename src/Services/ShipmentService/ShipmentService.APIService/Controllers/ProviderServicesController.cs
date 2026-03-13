using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/shipping")]
public class ProviderServicesController : ControllerBase
{
    private readonly IProviderServiceService _serviceService;

    public ProviderServicesController(IProviderServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpPost("providers/{provider_id:guid}/services")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> Create(
        [FromRoute(Name = "provider_id")] Guid providerId,
        [FromBody] CreateProviderServiceDto dto)
    {
        var result = await _serviceService.CreateAsync(providerId, dto);

        return result.Status switch
        {
            201 => StatusCode(201, result),
            404 => NotFound(result),
            409 => Conflict(result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpPatch("services/{service_id:guid}")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> Update(
        [FromRoute(Name = "service_id")] Guid serviceId,
        [FromBody] UpdateProviderServiceDto dto)
    {
        var result = await _serviceService.UpdateAsync(serviceId, dto);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            409 => Conflict(result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpGet("services")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServicePagedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<ProviderServicePagedDto>>> GetAll(
        [FromQuery] Guid? provider_id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var query = new GetServicesQueryDto
        {
            ProviderId = provider_id,
            Page = page,
            Limit = limit
        };
        var result = await _serviceService.GetPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("services/by-code")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServicePagedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResult<ProviderServicePagedDto>>> GetByCode(
        [FromQuery] string code,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(ServiceResult<ProviderServicePagedDto>.BadRequest("query param 'code' is required."));

<<<<<<< HEAD
    [HttpGet("GetServicesByProvider/{providerId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetByProvider(Guid providerId)
    {
        var result = await _providerServiceService.GetByProviderIdAsync(providerId);
        return Ok(result);
    }

    [HttpGet("GetByShopAndCode/{shopId:guid}/{code}")]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> GetByShopAndCode(Guid shopId, string code)
    {
        var result = await _providerServiceService.GetByShopIdAndCodeAsync(shopId, code);
        if (result.Status == 404) return NotFound(result);
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
=======
        var query = new GetServicesByCodeQueryDto
        {
            Code = code,
            Page = page,
            Limit = limit
        };
        var result = await _serviceService.GetByCodeAsync(query);
>>>>>>> develop
        return Ok(result);
    }
}
