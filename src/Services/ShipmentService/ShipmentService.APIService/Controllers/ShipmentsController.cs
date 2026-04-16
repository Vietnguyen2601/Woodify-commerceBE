using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

/// <summary>
/// Seller / ops: manage shipments for orders. REST-style routes under <c>/api/shipment/shipments</c>.
/// </summary>
[ApiController]
[Route("api/shipment/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShipmentDto>>>> GetAll()
    {
        var result = await _shipmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> GetById(Guid id)
    {
        var result = await _shipmentService.GetByIdAsync(id);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    /// <summary>List shipments for one order (typically zero or one active).</summary>
    [HttpGet("by-order/{orderId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShipmentDto>>>> GetByOrderId(Guid orderId)
    {
        var result = await _shipmentService.GetByOrderIdAsync(orderId);
        return Ok(result);
    }

    /// <summary>Seller: list shipments for a shop, newest first. Optional <paramref name="status"/> filter (e.g. IN_TRANSIT).</summary>
    [HttpGet("by-shop/{shopId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShipmentDto>>>> GetByShopId(
        Guid shopId,
        [FromQuery] string? status = null)
    {
        var result = await _shipmentService.GetByShopIdAsync(shopId, status);
        if (result.Status == 400) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Create shipment after order exists and order.created populated the cache.</summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> Create([FromBody] CreateShipmentDto dto)
    {
        var result = await _shipmentService.CreateAsync(dto);
        return result.Status switch
        {
            201 => CreatedAtAction(nameof(GetById), new { id = result.Data?.ShipmentId }, result),
            404 => NotFound(result),
            409 => Conflict(result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>Partial update: tracking, addresses, weight, fee, dates, failure/cancel reasons.</summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> Update(Guid id, [FromBody] UpdateShipmentDto dto)
    {
        var result = await _shipmentService.UpdateAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status == 400) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Change lifecycle status with server-side transition rules. Optional <see cref="UpdateShipmentStatusDto.FailureReason"/> / <see cref="UpdateShipmentStatusDto.CancelReason"/> when moving to DELIVERY_FAILED / CANCELLED.</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusDto dto)
    {
        var result = await _shipmentService.UpdateStatusAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status == 400) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>Mark picked up; sets <see cref="ShipmentDto.PickedUpAt"/> and moves status from PENDING/PICKUP_SCHEDULED to PICKED_UP.</summary>
    [HttpPatch("{id:guid}/pickup")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> MarkPickedUp(Guid id, [FromBody] UpdateShipmentPickupDto dto)
    {
        var result = await _shipmentService.UpdatePickupAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status == 400) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _shipmentService.DeleteAsync(id);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }
}
