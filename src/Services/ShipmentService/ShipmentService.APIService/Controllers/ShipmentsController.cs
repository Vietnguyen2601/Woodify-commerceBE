using Microsoft.AspNetCore.Mvc;

namespace ShipmentService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet("GetAllShipments")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShipmentDto>>>> GetAll()
    {
        var result = await _shipmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetShipmentById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> GetById(Guid id)
    {
        var result = await _shipmentService.GetByIdAsync(id);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    [HttpGet("GetShipmentsByOrderId/{orderId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ShipmentDto>>>> GetByOrderId(Guid orderId)
    {
        var result = await _shipmentService.GetByOrderIdAsync(orderId);
        return Ok(result);
    }

    [HttpPost("CreateShipment")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> Create([FromBody] CreateShipmentDto dto)
    {
        var result = await _shipmentService.CreateAsync(dto);
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.ShipmentId }, result);
        return BadRequest(result);
    }

    [HttpPut("UpdateShipment/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> Update(Guid id, [FromBody] UpdateShipmentDto dto)
    {
        var result = await _shipmentService.UpdateAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("UpdateShipmentStatus/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusDto dto)
    {
        var result = await _shipmentService.UpdateStatusAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("MarkShipmentPickedUp/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ShipmentDto>>> UpdatePickup(Guid id, [FromBody] UpdateShipmentPickupDto dto)
    {
        var result = await _shipmentService.UpdatePickupAsync(id, dto);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("DeleteShipment/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _shipmentService.DeleteAsync(id);
        if (result.Status == 404) return NotFound(result);
        if (result.Status != 200) return BadRequest(result);
        return Ok(result);
    }
}
