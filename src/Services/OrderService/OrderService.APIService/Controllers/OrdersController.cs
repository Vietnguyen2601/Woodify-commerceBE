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
    /// Tạo order từ giỏ hàng
    /// </summary>
    /// <param name="dto">Thông tin tạo order</param>
    /// <returns>Thông tin order đã tạo</returns>
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
}
