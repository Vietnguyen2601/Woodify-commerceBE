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

        var query = new GetServicesByCodeQueryDto
        {
            Code = code,
            Page = page,
            Limit = limit
        };
        var result = await _serviceService.GetByCodeAsync(query);
        return Ok(result);
    }
}
