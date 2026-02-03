using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductVersionCacheRepository _productCacheRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IProductVersionCacheRepository productCacheRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productCacheRepository = productCacheRepository;
    }

    public async Task<ServiceResult<OrderDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto)
    {
        try
        {
            // 1. Get cart with items
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(dto.AccountId);
            if (cart == null || !cart.CartItems.Any())
            {
                return ServiceResult<OrderDto>.NotFound("Cart is empty or not found");
            }

            // 2. Validate all cart items against product cache
            var invalidItems = new List<string>();
            var validItems = new List<CartItem>();
            Guid? shopId = null;

            foreach (var cartItem in cart.CartItems)
            {
                var productCache = await _productCacheRepository.GetByIdAsync(cartItem.ProductVersionId);
                
                if (productCache == null)
                {
                    invalidItems.Add($"Product version {cartItem.ProductVersionId} not found in cache");
                    continue;
                }

                if (productCache.ProductStatus != "PUBLISHED")
                {
                    invalidItems.Add($"Product '{cartItem.Title}' is not published (status: {productCache.ProductStatus})");
                    continue;
                }

                // For now, assume all products belong to the same shop
                // In real scenario, might need to split orders by shop
                if (shopId == null)
                {
                    shopId = productCache.ProductId; // Using ProductId as ShopId temporarily
                }

                validItems.Add(cartItem);
            }

            if (!validItems.Any())
            {
                return ServiceResult<OrderDto>.BadRequest($"No valid items in cart. Issues: {string.Join(", ", invalidItems)}");
            }

            // 3. Generate unique order code
            var orderCode = await GenerateUniqueOrderCodeAsync();

            // 4. Calculate totals
            long subtotalCents = validItems.Sum(item => item.UnitPriceCents * item.Qty);
            long shippingFeeCents = 0; // TODO: Calculate shipping fee
            long totalAmountCents = subtotalCents + shippingFeeCents;

            // 5. Create Order entity
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderCode = orderCode,
                AccountId = dto.AccountId,
                ShopId = shopId ?? Guid.Empty,
                Currency = "VND",
                SubtotalCents = subtotalCents,
                ShippingFeeCents = shippingFeeCents,
                TotalAmountCents = totalAmountCents,
                Status = OrderStatus.PENDING,
                PlacedAt = DateTime.UtcNow,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                ShippingAddress = dto.ShippingAddress,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Create Order Items (snapshot from cart items)
            foreach (var cartItem in validItems)
            {
                var productCache = await _productCacheRepository.GetByIdAsync(cartItem.ProductVersionId);
                
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = productCache?.ProductId,
                    ProductVersionId = cartItem.ProductVersionId,
                    SkuCode = cartItem.SkuCode,
                    Title = cartItem.Title,
                    UnitPriceCents = cartItem.UnitPriceCents,
                    Qty = cartItem.Qty,
                    TaxCents = 0,
                    LineTotalCents = cartItem.UnitPriceCents * cartItem.Qty,
                    FulfillmentStatus = FulfillmentStatus.UNFULFILLED,
                    ReturnedQty = 0,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
            }

            // 7. Save order to database
            await _orderRepository.CreateAsync(order);

            // 8. Clear cart after successful order creation
            var cartItemsToRemove = cart.CartItems.ToList();
            foreach (var cartItem in cartItemsToRemove)
            {
                await _cartItemRepository.RemoveAsync(cartItem);
            }

            // 9. Map to DTO and return
            var orderDto = MapToOrderDto(order);
            
            return ServiceResult<OrderDto>.Success(orderDto, "Order created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error creating order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            
            if (order == null)
            {
                return ServiceResult<OrderDto>.NotFound("Order not found");
            }

            var orderDto = MapToOrderDto(order);
            return ServiceResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error getting order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> GetOrderByCodeAsync(string orderCode)
    {
        try
        {
            var order = await _orderRepository.GetOrderByCodeAsync(orderCode);
            
            if (order == null)
            {
                return ServiceResult<OrderDto>.NotFound("Order not found");
            }

            var orderDto = MapToOrderDto(order);
            return ServiceResult<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error getting order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetOrdersByAccountIdAsync(Guid accountId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByAccountIdAsync(accountId);
            
            var orderDtos = orders.Select(o => MapToOrderDto(o)).ToList();
            
            return ServiceResult<List<OrderDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<OrderDto>>.InternalServerError($"Error getting orders: {ex.Message}");
        }
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            OrderCode = order.OrderCode,
            AccountId = order.AccountId,
            ShopId = order.ShopId,
            Currency = order.Currency,
            SubtotalCents = order.SubtotalCents,
            ShippingFeeCents = order.ShippingFeeCents,
            TotalAmountCents = order.TotalAmountCents,
            Status = order.Status.ToString(),
            PlacedAt = order.PlacedAt,
            CompletedAt = order.CompletedAt,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductVersionId = oi.ProductVersionId,
                SkuCode = oi.SkuCode,
                Title = oi.Title,
                UnitPriceCents = oi.UnitPriceCents,
                Qty = oi.Qty,
                TaxCents = oi.TaxCents,
                LineTotalCents = oi.LineTotalCents,
                FulfillmentStatus = oi.FulfillmentStatus.ToString(),
                ReturnedQty = oi.ReturnedQty,
                CreatedAt = oi.CreatedAt
            }).ToList()
        };
    }

    /// <summary>
    /// Sinh mã đơn hàng duy nhất với format: ORD-YYYYMMDD-XXXX
    /// XXXX là 4 chữ số ngẫu nhiên KHÔNG trùng lặp (ví dụ: 1234, 5678, không được 0012, 1123)
    /// </summary>
    private async Task<string> GenerateUniqueOrderCodeAsync()
    {
        var random = new Random();
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        string orderCode;
        bool isUnique = false;
        int maxAttempts = 100; // Giới hạn số lần thử để tránh infinite loop
        int attempts = 0;

        do
        {
            // Generate 4 unique random digits (no duplicates)
            var digits = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var selectedDigits = new List<int>();
            
            for (int i = 0; i < 4; i++)
            {
                int index = random.Next(digits.Count);
                selectedDigits.Add(digits[index]);
                digits.RemoveAt(index);
            }
            
            var randomDigits = string.Join("", selectedDigits);
            orderCode = $"ORD-{today}-{randomDigits}";

            // Check if order code already exists
            var existingOrder = await _orderRepository.GetOrderByCodeAsync(orderCode);
            isUnique = existingOrder == null;

            attempts++;
            if (attempts >= maxAttempts && !isUnique)
            {
                // Fallback to timestamp-based code if too many collisions
                var timestamp = DateTime.UtcNow.Ticks % 10000;
                orderCode = $"ORD-{today}-{timestamp:D4}";
                break;
            }
        }
        while (!isUnique);

        return orderCode;
    }
}
