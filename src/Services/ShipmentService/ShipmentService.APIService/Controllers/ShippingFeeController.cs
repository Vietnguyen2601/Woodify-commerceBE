using Microsoft.AspNetCore.Mvc;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using Shared.Results;

namespace ShipmentService.APIService.Controllers;

/// <summary>
/// Controller xử lý các API liên quan đến tính phí vận chuyển.
/// </summary>
[ApiController]
[Route("api/shipment")]
public class ShippingFeeController : ControllerBase
{
    private readonly IShippingFeePreviewService _feePreviewService;

    public ShippingFeeController(IShippingFeePreviewService feePreviewService)
    {
        _feePreviewService = feePreviewService;
    }

    /// <summary>Preview shipping fee for a selected service — used at checkout.</summary>
    [HttpPost("fee-preview")]
    [ProducesResponseType(typeof(ServiceResult<ShippingFeePreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<ShippingFeePreviewResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<ShippingFeePreviewResponse>>> PreviewShippingFee(
        [FromBody] ShippingFeePreviewRequest request)
    {
        var result = await _feePreviewService.CalculateAsync(request);

        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result)
        };
    }
}
