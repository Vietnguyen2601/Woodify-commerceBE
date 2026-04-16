using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace OrderService.APIService.Controllers;

[ApiController]
[Route("api/order/[controller]")]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Lấy giỏ hàng của người dùng
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <returns>Thông tin giỏ hàng</returns>
    [HttpGet("GetCart/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult<CartDto>>> GetCart(Guid accountId)
    {
        var result = await _cartService.GetCartByAccountIdAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    /// Thêm sản phẩm vào giỏ hàng
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <param name="dto">Thông tin sản phẩm cần thêm</param>
    /// <returns>Giỏ hàng sau khi cập nhật</returns>
    [HttpPost("AddToCart/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult<CartDto>>> AddToCart(Guid accountId, [FromBody] AddToCartDto dto)
    {
        var result = await _cartService.AddToCartAsync(accountId, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status == 400)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong giỏ hàng
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <param name="dto">Thông tin cập nhật</param>
    /// <returns>Giỏ hàng sau khi cập nhật</returns>
    [HttpPut("UpdateCartItem/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult<CartDto>>> UpdateCartItem(Guid accountId, [FromBody] UpdateCartItemDto dto)
    {
        var result = await _cartService.UpdateCartItemAsync(accountId, dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status == 400)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Xóa sản phẩm khỏi giỏ hàng
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <param name="cartItemId">ID của cart item cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("RemoveCartItem/{accountId:guid}/{cartItemId:guid}")]
    public async Task<ActionResult<ServiceResult>> RemoveCartItem(Guid accountId, Guid cartItemId)
    {
        var result = await _cartService.RemoveCartItemAsync(accountId, cartItemId);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Xóa toàn bộ giỏ hàng
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("ClearCart/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult>> ClearCart(Guid accountId)
    {
        var result = await _cartService.ClearCartAsync(accountId);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("CheckoutPreview/{accountId:guid}")]
    public async Task<IActionResult> GetCheckoutPreview(Guid accountId)
    {
        var result = await _cartService.GetCheckoutPreviewAsync(accountId);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }
}
