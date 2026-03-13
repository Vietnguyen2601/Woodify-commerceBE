using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;
using Shared.Events;

namespace OrderService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductVersionCacheRepository _productCacheRepository;
    private readonly OrderEventPublisher _orderEventPublisher;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IProductVersionCacheRepository productCacheRepository,
        OrderEventPublisher orderEventPublisher)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productCacheRepository = productCacheRepository;
        _orderEventPublisher = orderEventPublisher;
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

            // 2. Validate all cart items comprehensively
            var validationResult = await ValidateCartItemsForCheckoutAsync(cart.CartItems.ToList());

            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.InvalidItems.Select(item =>
                    $"Version {item.VersionId}: {item.Reason} - {item.Details}").ToList();

                return ServiceResult<OrderDto>.BadRequest(
                    errorDetails,
                    $"Cannot create order. {validationResult.InvalidItemsCount} of {validationResult.TotalItems} item(s) invalid."
                );
            }

            // 3. Get valid cart items
            var validCartItemIds = cart.CartItems
                .Select(ci => ci.CartItemId)
                .Except(validationResult.InvalidItems.Select(ii => ii.CartItemId))
                .ToHashSet();

            var validItems = cart.CartItems.Where(ci => validCartItemIds.Contains(ci.CartItemId)).ToList();

            if (!validItems.Any())
            {
                return ServiceResult<OrderDto>.BadRequest("No valid items to create order");
            }

            // 4. Calculate totals
            double subtotalCents = validItems.Sum(item => item.Price * item.Quantity);
            double totalAmountCents = subtotalCents; // Can add shipping, tax, discount calculations here

            // 5. Create Order entity
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = dto.AccountId,
                ShopId = dto.ShopId,
                SubtotalCents = subtotalCents,
                TotalAmountCents = totalAmountCents,
                VoucherId = dto.VoucherId,
                Payment = dto.Payment,
                Status = OrderStatus.PENDING,
                DeliveryAddressId = dto.DeliveryAddressId,
                CreatedAt = DateTime.UtcNow
            };

            // 6. Create Order Items (snapshot from cart items)
            foreach (var cartItem in validItems)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    VersionId = cartItem.VersionId,
                    UnitPriceCents = (long)(cartItem.Price * 100), // Convert to cents for precision
                    Quantity = cartItem.Quantity,
                    DiscountCents = 0,
                    TaxCents = 0,
                    LineTotalCents = cartItem.Price * cartItem.Quantity,
                    Status = FulfillmentStatus.UNFULFILLED,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
            }

            // 7. Save order to database
            await _orderRepository.CreateAsync(order);

            // 8. Publish OrderCreated event to RabbitMQ for ShipmentService
            _orderEventPublisher.PublishOrderCreated(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                AccountId = order.AccountId,
                DeliveryAddressId = order.DeliveryAddressId,
                TotalAmountCents = order.TotalAmountCents,
                CreatedAt = order.CreatedAt
            });

            // 9. Clear cart after successful order creation
            var cartItemsToRemove = cart.CartItems.ToList();
            foreach (var cartItem in cartItemsToRemove)
            {
                await _cartItemRepository.RemoveAsync(cartItem);
            }

            // 10. Map to DTO and return
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
            AccountId = order.AccountId,
            ShopId = order.ShopId,
            SubtotalCents = order.SubtotalCents,
            TotalAmountCents = order.TotalAmountCents,
            VoucherId = order.VoucherId,
            Payment = order.Payment,
            Status = order.Status.ToString(),
            DeliveryAddressId = order.DeliveryAddressId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                VersionId = oi.VersionId,
                UnitPriceCents = oi.UnitPriceCents,
                Quantity = oi.Quantity,
                DiscountCents = oi.DiscountCents,
                TaxCents = oi.TaxCents,
                LineTotalCents = oi.LineTotalCents,
                ShipmentId = oi.ShipmentId,
                Status = oi.Status.ToString(),
                CreatedAt = oi.CreatedAt
            }).ToList()
        };
    }

    /// <summary>
    /// Validate cart items cho checkout với các điều kiện:
    /// 1. Product vẫn PUBLISHED
    /// 2. Version chưa bị lock (kiểm tra status)
    /// 3. Giá hiện tại = giá snapshot (cảnh báo nếu khác)
    /// </summary>
    private async Task<CheckoutValidationDto> ValidateCartItemsForCheckoutAsync(List<CartItem> cartItems)
    {
        var result = new CheckoutValidationDto
        {
            TotalItems = cartItems.Count
        };

        foreach (var cartItem in cartItems)
        {
            var productCache = await _productCacheRepository.GetByVersionIdAsync(cartItem.VersionId);

            // Validate 1: Product version exists in cache
            if (productCache == null)
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Product not found",
                    Details = "Product version no longer exists or has been removed"
                });
                continue;
            }

            // Validate 1b: Product version not deleted
            if (productCache.IsDeleted)
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Product deleted",
                    Details = $"Product version was deleted on {productCache.DeletedAt?.ToString("yyyy-MM-dd HH:mm")}"
                });
                continue;
            }

            // Validate 2: Product status is PUBLISHED
            if (productCache.ProductStatus != "PUBLISHED")
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Product unavailable",
                    Details = $"Product status is {productCache.ProductStatus}. Only PUBLISHED products can be ordered"
                });
                continue;
            }

            // Validate 2b: Product version is active
            if (!productCache.IsActive)
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Version inactive",
                    Details = "This product version is no longer active"
                });
                continue;
            }

            // Validate 2c: Check stock availability
            if (productCache.StockQuantity < cartItem.Quantity && !productCache.AllowBackorder)
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Insufficient stock",
                    Details = $"Available stock: {productCache.StockQuantity}, Requested: {cartItem.Quantity}"
                });
                continue;
            }

            // Validate 3: Version not locked (assuming ARCHIVED status means locked)
            if (productCache.ProductStatus == "ARCHIVED")
            {
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Version locked",
                    Details = "This product version has been archived and is no longer available"
                });
                continue;
            }

            // Validate 4: Price comparison (current vs snapshot)
            if (Math.Abs(productCache.Price - cartItem.Price) > 0.01) // Small tolerance for floating point
            {
                var priceDifference = productCache.Price - cartItem.Price;
                var percentageChange = Math.Abs(decimal.Parse((priceDifference / cartItem.Price * 100).ToString()));

                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Price changed",
                    Details = $"Price changed by {percentageChange:F1}%. Cart price: {cartItem.Price}, Current price: {productCache.Price}"
                });
                continue;
            }
        }

        result.InvalidItemsCount = result.InvalidItems.Count;
        result.ValidItemsCount = result.TotalItems - result.InvalidItemsCount;
        result.IsValid = result.InvalidItemsCount == 0;

        return result;
    }
}
