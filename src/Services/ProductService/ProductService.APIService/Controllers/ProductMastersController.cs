using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace ProductService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductMastersController : ControllerBase
{
    private readonly IProductMasterService _productMasterService;

    public ProductMastersController(IProductMasterService productMasterService)
    {
        _productMasterService = productMasterService;
    }

    [HttpGet("GetAllProducts")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductMasterDto>>>> GetAll()
    {
        var result = await _productMasterService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetProductById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> GetById(Guid id)
    {
        var result = await _productMasterService.GetByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetProductByGlobalSku/{globalSku}")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> GetByGlobalSku(string globalSku)
    {
        var result = await _productMasterService.GetByGlobalSkuAsync(globalSku);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetProductsByShopId/{shopId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductMasterDto>>>> GetByShopId(Guid shopId)
    {
        var result = await _productMasterService.GetByShopIdAsync(shopId);
        return Ok(result);
    }

    [HttpPost("CreateProduct")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> Create([FromBody] CreateProductMasterDto dto)
    {
        var result = await _productMasterService.CreateAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.ProductId }, result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateProduct/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> Update(Guid id, [FromBody] UpdateProductMasterDto dto)
    {
        var result = await _productMasterService.UpdateAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("DeleteProduct/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _productMasterService.DeleteAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPatch("ArchiveProduct/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> ArchiveProduct(Guid id)
    {
        var result = await _productMasterService.ArchiveProductAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("GetArchivedProducts")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductMasterDto>>>> GetArchivedProducts()
    {
        var result = await _productMasterService.GetArchivedProductsAsync();
        return Ok(result);
    }

    [HttpPatch("PublishProduct/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductMasterDto>>> PublishProduct(Guid id)
    {
        var result = await _productMasterService.PublishProductAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpGet("GetPublishedProducts")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductMasterDto>>>> GetPublishedProducts()
    {
        var result = await _productMasterService.GetPublishedProductsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Search products with filters
    /// Only indexes PUBLISHED products by default
    /// </summary>
    [HttpPost("SearchProducts")]
    public async Task<ActionResult<ServiceResult<ProductSearchResultDto>>> SearchProducts([FromBody] ProductSearchDto searchDto)
    {
        var result = await _productMasterService.SearchProductsAsync(searchDto);
        return Ok(result);
    }
}
