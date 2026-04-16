using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace ProductService.APIService.Controllers;

[ApiController]
[Route("api/product/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("GetAllCategories")]
    public async Task<ActionResult<ServiceResult<IEnumerable<CategoryDto>>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetCategoryById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<CategoryDto>>> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetCategoryByName/{name}")]
    public async Task<ActionResult<ServiceResult<CategoryDto>>> GetByName(string name)
    {
        var result = await _categoryService.GetByNameAsync(name);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetRootCategories")]
    public async Task<ActionResult<ServiceResult<IEnumerable<CategoryDto>>>> GetRootCategories()
    {
        var result = await _categoryService.GetRootCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("GetSubCategories/{parentCategoryId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<CategoryDto>>>> GetSubCategories(Guid parentCategoryId)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentCategoryId);
        return Ok(result);
    }

    [HttpGet("GetActiveCategories")]
    public async Task<ActionResult<ServiceResult<IEnumerable<CategoryDto>>>> GetActiveCategories()
    {
        var result = await _categoryService.GetActiveCategoriesAsync();
        return Ok(result);
    }

    [HttpPost("CreateCategory")]
    public async Task<ActionResult<ServiceResult<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.CategoryId }, result);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateCategory/{id:guid}")]
    public async Task<ActionResult<ServiceResult<CategoryDto>>> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status == 400)
            return BadRequest(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("DeleteCategory/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status == 400)
            return BadRequest(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }
}
