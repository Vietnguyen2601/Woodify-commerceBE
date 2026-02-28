using OrderService.Application.DTOs;
using Shared.Results;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface cho Order Business Service
/// </summary>
public interface IOrderService
{
    Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto);
    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByAccountIdAsync(Guid accountId);
}
