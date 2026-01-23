using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace ProductService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewService _productReviewService;

    public ProductReviewsController(IProductReviewService productReviewService)
    {
        _productReviewService = productReviewService;
    }

    [HttpGet("GetAllReviews")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductReviewDto>>>> GetAll()
    {
        var result = await _productReviewService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetReviewById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductReviewDto>>> GetById(Guid id)
    {
        var result = await _productReviewService.GetByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpGet("GetReviewsByProductId/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductReviewDto>>>> GetByProductId(Guid productId)
    {
        var result = await _productReviewService.GetByProductIdAsync(productId);
        return Ok(result);
    }

    [HttpGet("GetReviewsByAccountId/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductReviewDto>>>> GetByAccountId(Guid accountId)
    {
        var result = await _productReviewService.GetByAccountIdAsync(accountId);
        return Ok(result);
    }

    [HttpGet("GetReviewsByOrderId/{orderId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductReviewDto>>>> GetByOrderId(Guid orderId)
    {
        var result = await _productReviewService.GetByOrderIdAsync(orderId);
        return Ok(result);
    }

    [HttpGet("GetVerifiedReviews/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<IEnumerable<ProductReviewDto>>>> GetVerifiedReviews(Guid productId)
    {
        var result = await _productReviewService.GetVerifiedReviewsAsync(productId);
        return Ok(result);
    }

    [HttpPost("CreateReview")]
    public async Task<ActionResult<ServiceResult<ProductReviewDto>>> Create([FromBody] CreateProductReviewDto dto)
    {
        var result = await _productReviewService.CreateAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.ReviewId }, result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateReview/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductReviewDto>>> Update(Guid id, [FromBody] UpdateProductReviewDto dto)
    {
        var result = await _productReviewService.UpdateAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpPost("IncrementHelpful/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ProductReviewDto>>> IncrementHelpfulCount(Guid id)
    {
        var result = await _productReviewService.IncrementHelpfulCountAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("DeleteReview/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _productReviewService.DeleteAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }
}
