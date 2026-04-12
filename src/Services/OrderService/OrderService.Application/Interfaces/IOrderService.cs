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
    /// <summary>
    /// Tạo Order từ 1 shop (v2 refactored)
    /// Frontend gọi N lần cho N shops, mỗi lần process 1 shop
    /// </summary>
    Task<ServiceResult<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request);

    /// <summary>ECO / STD / EXP fees + grand totals before placing order (matches create-order math).</summary>
    Task<ServiceResult<CheckoutShippingPreviewResponseDto>> PreviewCheckoutShippingAsync(CheckoutShippingPreviewRequest request);

    /// <summary>
    /// Legacy method - giữ cho backward compatibility (nếu cần)
    /// </summary>
    Task<ServiceResult<CreateOrdersFromCartResultDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto);

    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId);
    Task<ServiceResult<List<OrderDto>>> GetOrdersByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Danh sách đơn của account cho customer: line items + chi tiết sản ph��m (cache), có PaymentStatus, không ShipmentId.
    /// </summary>
    Task<ServiceResult<List<CustomerAccountOrderDto>>> GetOrdersByAccountForCustomerAsync(Guid accountId);

    Task<ServiceResult<List<OrderWithProductDetailsDto>>> GetOrdersByShopIdAsync(Guid shopId);
    Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
    Task<ServiceResult<OrderListResultDto>> GetAllOrdersAsync(GetAllOrdersQueryDto query);

    /// <summary>Top product masters by units sold; enriched from product_version_cache and shop_info_cache (no cross-service HTTP).</summary>
    Task<ServiceResult<List<TopSellingProductAnalyticsDto>>> GetTopSellingProductsAsync(int limit = 5, Guid? shopId = null);
}
