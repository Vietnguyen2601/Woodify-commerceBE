using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using Shared.Results;

namespace ProductService.APIService.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private readonly IImageUrlService _imageUrlService;

    public ImagesController(IImageUrlService imageUrlService)
    {
        _imageUrlService = imageUrlService;
    }

    /// <summary>
    /// FE upload ảnh lên Cloudinary trực tiếp, sau đó gửi metadata xuống đây để lưu DB.
    /// imageType hợp lệ: PRODUCT, PRODUCT_VERSION, REVIEW, AVATAR, SHOP_LOGO, SHOP_COVER, CATEGORY, BANNER, ADS
    /// </summary>
    [HttpPost("save")]
    public async Task<ActionResult<ServiceResult<ImageUrlDto>>> Save([FromBody] SaveImageUrlDto dto)
    {
        var result = await _imageUrlService.SaveImageAsync(dto);
        if (result.Status == 201) return StatusCode(201, result);
        return BadRequest(result);
    }

    /// <summary>Lưu nhiều ảnh cùng lúc (sau khi FE upload lên Cloudinary)</summary>
    [HttpPost("save-bulk")]
    public async Task<ActionResult<ServiceResult<List<ImageUrlDto>>>> SaveBulk([FromBody] BulkSaveImageUrlsDto dto)
    {
        var result = await _imageUrlService.SaveImagesAsync(dto);
        if (result.Status == 201) return StatusCode(201, result);
        return BadRequest(result);
    }

    /// <summary>Lấy tất cả ảnh của một Product, sắp xếp theo sort_order ASC. Ảnh đầu tiên là ảnh chính.</summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<ServiceResult<List<ImageUrlDto>>>> GetProductImages(Guid productId)
    {
        var result = await _imageUrlService.GetImagesAsync("PRODUCT", productId);
        return Ok(result);
    }

    /// <summary>Lấy ảnh chính (sort_order nhỏ nhất) của một Product.</summary>
    [HttpGet("product/{productId:guid}/primary")]
    public async Task<ActionResult<ServiceResult<ImageUrlDto>>> GetPrimaryProductImage(Guid productId)
    {
        var result = await _imageUrlService.GetPrimaryImageAsync("PRODUCT", productId);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Lấy tất cả ảnh của một Product Version, sắp xếp theo sort_order ASC.</summary>
    [HttpGet("version/{versionId:guid}")]
    public async Task<ActionResult<ServiceResult<List<ImageUrlDto>>>> GetVersionImages(Guid versionId)
    {
        var result = await _imageUrlService.GetImagesAsync("PRODUCT_VERSION", versionId);
        return Ok(result);
    }

    /// <summary>Lấy ảnh chính của một Product Version.</summary>
    [HttpGet("version/{versionId:guid}/primary")]
    public async Task<ActionResult<ServiceResult<ImageUrlDto>>> GetPrimaryVersionImage(Guid versionId)
    {
        var result = await _imageUrlService.GetPrimaryImageAsync("PRODUCT_VERSION", versionId);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Lấy tất cả ảnh của một Review, sắp xếp theo sort_order ASC.</summary>
    [HttpGet("review/{reviewId:guid}")]
    public async Task<ActionResult<ServiceResult<List<ImageUrlDto>>>> GetReviewImages(Guid reviewId)
    {
        var result = await _imageUrlService.GetImagesAsync("REVIEW", reviewId);
        return Ok(result);
    }

    /// <summary>Lấy ảnh chính của một Review.</summary>
    [HttpGet("review/{reviewId:guid}/primary")]
    public async Task<ActionResult<ServiceResult<ImageUrlDto>>> GetPrimaryReviewImage(Guid reviewId)
    {
        var result = await _imageUrlService.GetPrimaryImageAsync("REVIEW", reviewId);
        if (result.Status == 404) return NotFound(result);
        return Ok(result);
    }
}
