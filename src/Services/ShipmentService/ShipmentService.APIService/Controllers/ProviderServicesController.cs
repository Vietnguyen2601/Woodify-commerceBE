using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/shipment")]
public class ProviderServicesController : ControllerBase
{
    private readonly IProviderServiceService _serviceService;

    public ProviderServicesController(IProviderServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpPost("services")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServiceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> Create(
        [FromBody] CreateProviderServiceDto dto)
    {
        var result = await _serviceService.CreateAsync(dto);

        return result.Status switch
        {
            201 => StatusCode(201, result),
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
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProviderServiceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetAll()
    {
        var result = await _serviceService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("providers/{providerId:guid}/services")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProviderServiceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetByProviderId(
        [FromRoute] Guid providerId)
    {
        var result = await _serviceService.GetByProviderIdAsync(providerId);
        return Ok(result);
    }

    /// <summary>Dịch vụ vận chuyển của default provider theo shop (spec: services/by-shop).</summary>
    [HttpGet("services/by-shop/{shopId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProviderServiceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetServicesByShopId(
        [FromRoute] Guid shopId)
    {
        var result = await _serviceService.GetServicesByShopIdAsync(shopId);
        return result.Status switch
        {
            404 => NotFound(result),
            _ => Ok(result)
        };
    }

    /// <summary>Alias: GET shops/{shopId}/services (cùng handler).</summary>
    [HttpGet("shops/{shopId:guid}/services")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProviderServiceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ServiceResult<IEnumerable<ProviderServiceDto>>>> GetServicesByShopIdAlias(
        [FromRoute] Guid shopId) =>
        GetServicesByShopId(shopId);

    [HttpGet("services/{id:guid}")]
    [ProducesResponseType(typeof(ServiceResult<ProviderServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<ProviderServiceDto>>> GetById(
        [FromRoute] Guid id)
    {
        var result = await _serviceService.GetByIdAsync(id);
        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpDelete("services/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult>> Delete(
        [FromRoute] Guid id)
    {
        var result = await _serviceService.DeleteAsync(id);
        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }
}

