using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace OrderService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Tạo order từ giỏ hàng - hỗ trợ checkout selected items hoặc toàn bộ cart
    /// </summary>
    /// <remarks>
    /// Behavior:
    /// - Nếu SelectedCartItemIds = null/empty → checkout toàn bộ cart (backward compatible)
    /// - Nếu SelectedCartItemIds = [id1, id2, ...] → checkout chỉ những items này, items còn lại giữ trong cart
    /// - Validate: kiểm tra stock, giá, status sản phẩm
    /// - Split by shop: tạo order per shop từ selected items
    /// </remarks>
    /// <param name="dto">Thông tin tạo order với selectedCartItemIds tùy chọn</param>
    /// <returns>Order được tạo thành công</returns>
    [HttpPost("CreateFromCart")]
    public async Task<ActionResult<ServiceResult<OrderDto>>> CreateOrderFromCart(CreateOrderFromCartDto dto)
    {
        var result = await _orderService.CreateOrderFromCartAsync(dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status == 400)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy order theo ID
    /// </summary>
    /// <param name="orderId">ID của order</param>
    /// <returns>Thông tin order</returns>
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<ServiceResult<OrderDto>>> GetOrder(Guid orderId)
    {
        var result = await _orderService.GetOrderByIdAsync(orderId);

        if (result.Status == 404)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách orders của user
    /// </summary>
    /// <param name="accountId">ID của tài khoản</param>
    /// <returns>Danh sách orders</returns>
    [HttpGet("Account/{accountId:guid}")]
    public async Task<ActionResult<ServiceResult<List<OrderDto>>>> GetOrdersByAccount(Guid accountId)
    {
        var result = await _orderService.GetOrdersByAccountIdAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách orders của shop với chi tiết sản phẩm (cho seller)
    /// </summary>
    /// <param name="shopId">ID của shop</param>
    /// <returns>Danh sách orders kèm chi tiết sản phẩm</returns>
    [HttpGet("Shop/{shopId:guid}")]
    public async Task<ActionResult<ServiceResult<List<OrderWithProductDetailsDto>>>> GetOrdersByShop(Guid shopId)
    {
        var result = await _orderService.GetOrdersByShopIdAsync(shopId);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của order (cho seller/admin)
    /// </summary>
    /// <param name="dto">Thông tin cập nhật trạng thái</param>
    /// <returns>Order đã được cập nhật</returns>
    [HttpPut("UpdateStatus")]
    public async Task<ActionResult<ServiceResult<OrderDto>>> UpdateOrderStatus(UpdateOrderStatusDto dto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(dto);

        if (result.Status == 404)
            return NotFound(result);

        if (result.Status == 400)
            return BadRequest(result);

        return Ok(result);
    }
}
