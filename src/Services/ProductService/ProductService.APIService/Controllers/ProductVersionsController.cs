using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace ProductService.APIService.Controllers;

[ApiController]
[Route("api/product/[controller]")]
public class ProductVersionsController : ControllerBase
{
    private readonly IProductVersionService _productVersionService;

    public ProductVersionsController(IProductVersionService productVersionService)
    {
        _productVersionService = productVersionService;
    }

    [HttpGet("GetAllVersions")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductVersionDto>>>> GetAll()
    {
        var result = await _productVersionService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetVersionById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> GetById(Guid id)
    {
        var result = await _productVersionService.GetByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetVersionBySellerSku/{sellerSku}")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> GetBySellerSku(string sellerSku)
    {
        var result = await _productVersionService.GetBySellerSkuAsync(sellerSku);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetVersionsByProductId/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductVersionDto>>>> GetByProductId(Guid productId)
    {
        var result = await _productVersionService.GetByProductIdAsync(productId);
        return Ok(result);
    }

    [HttpGet("GetLatestVersionByProductId/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> GetLatestVersionByProductId(Guid productId)
    {
        var result = await _productVersionService.GetLatestVersionByProductIdAsync(productId);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetDefaultVersionByProductId/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> GetDefaultVersionByProductId(Guid productId)
    {
        var result = await _productVersionService.GetDefaultVersionByProductIdAsync(productId);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetInactiveVersions")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductVersionDto>>>> GetInactiveVersions()
    {
        var result = await _productVersionService.GetInactiveVersionsAsync();
        return Ok(result);
    }

    [HttpGet("GetActiveVersions")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductVersionDto>>>> GetActiveVersions()
    {
        var result = await _productVersionService.GetActiveVersionsAsync();
        return Ok(result);
    }

    [HttpPost("CreateVersion")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> Create([FromBody] CreateProductVersionDto dto)
    {
        var result = await _productVersionService.CreateAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.VersionId }, result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateVersion/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductVersionDto>>> Update(Guid id, [FromBody] UpdateProductVersionDto dto)
    {
        var result = await _productVersionService.UpdateAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPatch("DeactivateVersion/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Deactivate(Guid id)
    {
        var result = await _productVersionService.DeactivateAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPatch("ActivateVersion/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Activate(Guid id)
    {
        var result = await _productVersionService.ActivateAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPatch("SetAsDefault/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> SetAsDefault(Guid id)
    {
        var result = await _productVersionService.SetAsDefaultAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }
}
