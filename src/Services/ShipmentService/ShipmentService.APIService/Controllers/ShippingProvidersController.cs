using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/shipping")]
public class ShippingProvidersController : ControllerBase
{
    private readonly IShippingProviderService _providerService;

    public ShippingProvidersController(IShippingProviderService providerService)
    {
        _providerService = providerService;
    }

<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
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
<<<<<<< HEAD
=======
    [HttpPost("providers")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
>>>>>>> develop
=======
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
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

<<<<<<< HEAD
<<<<<<< HEAD
    [HttpPut("UpdateProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Update(Guid id, [FromBody] UpdateShippingProviderDto dto)
=======
    [HttpGet("providers")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderPagedDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<ShippingProviderPagedDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
>>>>>>> develop
=======
    [HttpPut("UpdateProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Update(Guid id, [FromBody] UpdateShippingProviderDto dto)
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
    {
        var query = new GetProvidersQueryDto { Page = page, Limit = limit };
        var result = await _providerService.GetPagedAsync(query);
        return Ok(result);
    }

<<<<<<< HEAD
<<<<<<< HEAD
    [HttpDelete("DeleteProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
=======
    [HttpPatch("providers/{provider_id:guid}")]
    [ProducesResponseType(typeof(ServiceResult<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<ShippingProviderDto>>> Update(
        [FromRoute(Name = "provider_id")] Guid providerId,
        [FromBody] UpdateShippingProviderDto dto)
>>>>>>> develop
=======
    [HttpDelete("DeleteProvider/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
>>>>>>> e9d308fc572a492ff112cf3ae8de135376051391
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
}
