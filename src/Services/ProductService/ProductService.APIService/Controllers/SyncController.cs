using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Shared.Results;

namespace ProductService.APIService.Controllers;

/// <summary>
/// Controller để sync/export dữ liệu cho các services khác
/// Dùng cho initial data synchronization hoặc manual re-sync
/// </summary>
[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IProductMasterService _productMasterService;

    public SyncController(ICategoryService categoryService, IProductMasterService productMasterService)
    {
        _categoryService = categoryService;
        _productMasterService = productMasterService;
    }

    /// <summary>
    /// Lấy tất cả categories cho sync/cache
    /// Dùng bởi OrderService để populate CategoryCache ban đầu
    /// </summary>
    /// <remarks>
    /// GET /api/sync/categories
    /// Trả về tất cả categories hiện có
    /// </remarks>
    /// <returns>List of all active categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<CategoryDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<IEnumerable<CategoryDto>>>> SyncCategories()
    {
        var result = await _categoryService.GetAllAsync();
        
        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy tất cả published products cho sync/cache
    /// Dùng bởi OrderService để populate ProductMasterCache ban đầu
    /// </summary>
    /// <remarks>
    /// GET /api/sync/products
    /// Trả về tất cả products hiện có
    /// </remarks>
    /// <returns>List of all published products</returns>
    [HttpGet("products")]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProductMasterDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<IEnumerable<ProductMasterDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductMasterDto>>>> SyncProducts()
    {
        var result = await _productMasterService.GetAllAsync();
        
        if (result.Status == 500)
            return StatusCode(500, result);

        return Ok(result);
    }
}
