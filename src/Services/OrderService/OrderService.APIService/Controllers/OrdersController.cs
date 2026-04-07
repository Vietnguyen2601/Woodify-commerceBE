using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace OrderService.APIService.Controllers;

[ApiController]
[Route("api/order/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Tạo orders từ giỏ hàng - hỗ trợ checkout selected items hoặc toàn bộ cart
    /// Trả về danh sách orderIds để frontend gửi tới PaymentService
    /// </summary>
    /// <remarks>
    /// Step 1 của payment flow:
    /// - Nhận request: accountId, selectedCartItemIds, providerServiceCode, paymentMethod
    /// - Tính commission (6%) + shipping fee cho mỗi order
    /// - Trả về danh sách orderIds (1 order per shop)
    /// - Frontend dùng orderIds này để gọi POST /api/payments/create
    /// </remarks>
    /// <param name="dto">Thông tin tạo orders</param>
    /// <returns>Danh sách orderIds + tổng tiền cần thanh toán</returns>
    [HttpPost("CreateFromCart")]
    [ProducesResponseType(typeof(ServiceResult<CreateOrdersFromCartResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<CreateOrdersFromCartResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<CreateOrdersFromCartResultDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<CreateOrdersFromCartResultDto>>> CreateOrderFromCart(
        [FromBody] CreateOrderFromCartDto dto)
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

    /// <summary>
    /// Admin: Lấy tất cả orders có pagination và filter
    /// </summary>
    /// <param name="page">Trang (mặc định 1)</param>
    /// <param name="pageSize">Số lượng mỗi trang (mặc định 20, tối đa 100)</param>
    /// <param name="status">Lọc theo trạng thái: PENDING, CONFIRMED, PROCESSING, READY_TO_SHIP, SHIPPED, DELIVERED, COMPLETED, CANCELLED, REFUNDING, REFUNDED</param>
    /// <param name="shopId">Lọc theo shop</param>
    /// <param name="accountId">Lọc theo customer</param>
    [HttpGet("GetAllOrders")]
    [ProducesResponseType(typeof(ServiceResult<OrderListResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<OrderListResultDto>>> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? shopId = null,
        [FromQuery] Guid? accountId = null)
    {
        var query = new GetAllOrdersQueryDto
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            ShopId = shopId,
            AccountId = accountId
        };

        var result = await _orderService.GetAllOrdersAsync(query);
        return Ok(result);
    }
}
