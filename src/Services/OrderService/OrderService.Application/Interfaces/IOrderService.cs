using OrderService.Application.DTOs;
using Shared.Results;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface cho Order Business Service
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Tạo múltiple orders từ cart (1 order per shop)
    /// Tính commission + shipping fee cho mỗi order
    /// Trả về danh sách orderIds để frontend gửi tới PaymentService
    /// </summary>
    Task<ServiceResult<CreateOrdersFromCartResultDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto);

    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByAccountIdAsync(Guid accountId);
    Task<ServiceResult<List<OrderWithProductDetailsDto>>> GetOrdersByShopIdAsync(Guid shopId);
    Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
    Task<ServiceResult<OrderListResultDto>> GetAllOrdersAsync(GetAllOrdersQueryDto query);
}
