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

            // 4. Get shop ID from first valid item (all items should be from same shop)
            Guid shopId = validItems.First().ShopId;
            string shopName = "Shop Name"; // TODO: Get from ShopService

            // 5. Parse payment method
            if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, true, out var paymentMethod))
            {
                return ServiceResult<OrderDto>.BadRequest("Invalid payment method");
            }

            // 6. Generate unique order code
            var orderCode = await GenerateUniqueOrderCodeAsync();

            // 7. Calculate totals
            long subtotalCents = validItems.Sum(item => item.UnitPriceCents * item.Quantity);
            long shippingFeeCents = 0; // TODO: Calculate shipping fee
            long discountCents = 0; // TODO: Apply voucher discount
            long taxCents = 0; // TODO: Calculate tax
            long totalAmountCents = subtotalCents + shippingFeeCents - discountCents + taxCents;

            // 8. Create Order entity
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderCode = orderCode,
                AccountId = dto.AccountId,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                ShopId = shopId,
                ShopName = shopName,
                Currency = "VND",
                SubtotalCents = subtotalCents,
                ShippingFeeCents = shippingFeeCents,
                DiscountCents = discountCents,
                TaxCents = taxCents,
                TotalAmountCents = totalAmountCents,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.PENDING,
                Status = OrderStatus.PENDING,
                DeliveryAddressId = dto.DeliveryAddressId,
                CustomerNote = dto.CustomerNote,
                PlacedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // 9. Create Order Items (snapshot from cart items)
            foreach (var cartItem in validItems)
            {
                var productCache = await _productCacheRepository.GetByVersionIdAsync(cartItem.VersionId);
                
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    VersionId = cartItem.VersionId,
                    ProductName = productCache?.ProductName ?? "Unknown Product",
                    VariantName = productCache?.VersionName,
                    SellerSku = productCache?.SellerSku ?? "",
                    UnitPriceCents = cartItem.UnitPriceCents,
                    Quantity = cartItem.Quantity,
                    DiscountCents = 0,
                    TaxCents = 0,
                    LineTotalCents = cartItem.UnitPriceCents * cartItem.Quantity,
                    Status = FulfillmentStatus.UNFULFILLED,
                    ReturnedQuantity = 0,
                    RefundedAmountCents = 0,
                    ShippingInfo = productCache != null 
                        ? $"Weight: {productCache.WeightGrams}g, Dimensions: {productCache.LengthCm}x{productCache.WidthCm}x{productCache.HeightCm}cm"
                        : null,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
            }

            // 10. Save order to database
            await _orderRepository.CreateAsync(order);

            // 11. Clear cart after successful order creation
            var cartItemsToRemove = cart.CartItems.ToList();
            foreach (var cartItem in cartItemsToRemove)
            {
                await _cartItemRepository.RemoveAsync(cartItem);
            }

            // 12. Map to DTO and return
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
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            ShopId = order.ShopId,
            ShopName = order.ShopName,
            Currency = order.Currency,
            SubtotalCents = order.SubtotalCents,
            ShippingFeeCents = order.ShippingFeeCents,
            DiscountCents = order.DiscountCents,
            TaxCents = order.TaxCents,
            TotalAmountCents = order.TotalAmountCents,
            VoucherApplied = order.VoucherApplied,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentTransactionId = order.PaymentTransactionId,
            Status = order.Status.ToString(),
            DeliveryAddressId = order.DeliveryAddressId,
            CustomerNote = order.CustomerNote,
            ShopNote = order.ShopNote,
            PlacedAt = order.PlacedAt,
            ConfirmedAt = order.ConfirmedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CompletedAt = order.CompletedAt,
            CancelledAt = order.CancelledAt,
            CancelReason = order.CancelReason,
            CancelledBy = order.CancelledBy,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                VersionId = oi.VersionId,
                ProductName = oi.ProductName,
                VariantName = oi.VariantName,
                SellerSku = oi.SellerSku,
                UnitPriceCents = oi.UnitPriceCents,
                Quantity = oi.Quantity,
                DiscountCents = oi.DiscountCents,
                TaxCents = oi.TaxCents,
                LineTotalCents = oi.LineTotalCents,
                ShipmentId = oi.ShipmentId,
                Status = oi.Status.ToString(),
                ReturnedQuantity = oi.ReturnedQuantity,
                RefundedAmountCents = oi.RefundedAmountCents,
                ShippingInfo = oi.ShippingInfo,
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
            if (productCache.PriceCents != cartItem.UnitPriceCents)
            {
                var priceDifference = productCache.PriceCents - cartItem.UnitPriceCents;
                var percentageChange = Math.Abs((decimal)priceDifference / cartItem.UnitPriceCents * 100);
                
                result.InvalidItems.Add(new InvalidCartItemDto
                {
                    CartItemId = cartItem.CartItemId,
                    VersionId = cartItem.VersionId,
                    Reason = "Price changed",
                    Details = $"Price changed by {percentageChange:F1}%. Cart price: {cartItem.UnitPriceCents}, Current price: {productCache.PriceCents}"
                });
                continue;
            }
        }

        result.InvalidItemsCount = result.InvalidItems.Count;
        result.ValidItemsCount = result.TotalItems - result.InvalidItemsCount;
        result.IsValid = result.InvalidItemsCount == 0;

        return result;
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
