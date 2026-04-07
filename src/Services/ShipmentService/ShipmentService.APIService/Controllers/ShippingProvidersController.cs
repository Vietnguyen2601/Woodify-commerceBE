using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/shipment")]
public class ShippingProvidersController : ControllerBase
{
    private readonly IShippingProviderService _providerService;

    public ShippingProvidersController(IShippingProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet("providers")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderPagedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<ShippingProviderPagedDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        var query = new GetProvidersQueryDto
        {
            Page = page,
            Limit = limit
        };
        var result = await _providerService.GetPagedAsync(query);
        return Ok(result);
    }

    [HttpPost("providers")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Create([FromBody] CreateShippingProviderDto dto)
    {
        var result = await _providerService.CreateAsync(dto);

        return result.Status switch
        {
            201 => StatusCode(201, result),
            409 => Conflict(result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpPut("providers/{providerId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Update(
        [FromRoute] Guid providerId,
        [FromBody] UpdateShippingProviderDto dto)
    {
        var result = await _providerService.UpdateAsync(providerId, dto);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            409 => Conflict(result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpGet("providers/{providerId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> GetById([FromRoute] Guid providerId)
    {
        var result = await _providerService.GetByIdAsync(providerId);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpDelete("providers/{providerId:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ServiceResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult>> Delete([FromRoute] Guid providerId)
    {
        var result = await _providerService.DeleteAsync(providerId);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            409 => Conflict(result),
            _ => StatusCode(result.Status, result)
        };
    }
}


