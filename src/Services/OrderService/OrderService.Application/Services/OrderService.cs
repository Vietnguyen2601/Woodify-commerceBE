using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;
using Shared.Events;
using Shared.Constants;

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
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                return ServiceResult<OrderDto>.BadRequest("Delivery address is required");

            // 2. Get cart with items
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(dto.AccountId);
            if (cart == null || !cart.CartItems.Any())
            {
                return ServiceResult<OrderDto>.NotFound("Cart is empty or not found");
            }

            // 3. FILTER ITEMS: Selected or All (BACKWARD COMPATIBLE)
            List<CartItem> itemsToProcess = GetItemsToProcess(
                cart.CartItems.ToList(),
                dto.SelectedCartItemIds
            );

            if (!itemsToProcess.Any())
                return ServiceResult<OrderDto>.BadRequest("No items to process");

            // 4. Validate selected IDs belong to this cart
            var validationError = ValidateSelectedItemsBelongToCart(itemsToProcess, dto.SelectedCartItemIds);
            if (validationError != null)
                return ServiceResult<OrderDto>.BadRequest(validationError);

            // 5. Validate all items comprehensively
            var validationResult = await ValidateCartItemsForCheckoutAsync(itemsToProcess);

            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.InvalidItems.Select(item =>
                    $"Version {item.VersionId}: {item.Reason}").ToList();

                return ServiceResult<OrderDto>.BadRequest(
                    errorDetails,
                    $"Cannot create order. {validationResult.InvalidItemsCount} of {validationResult.TotalItems} item(s) invalid."
                );
            }

            // 6. Get valid cart items
            var validCartItemIds = itemsToProcess
                .Select(ci => ci.CartItemId)
                .Except(validationResult.InvalidItems.Select(ii => ii.CartItemId))
                .ToHashSet();

            var validItems = itemsToProcess
                .Where(ci => validCartItemIds.Contains(ci.CartItemId))
                .ToList();

            if (!validItems.Any())
            {
                return ServiceResult<OrderDto>.BadRequest("No valid items to create order");
            }

            // 7. SPLIT BY SHOP & CREATE ORDERS FOR EACH SHOP
            // Tạo order riêng cho mỗi shop (hỗ trợ multi-shop checkout như Shopee, Lazada)
            var itemsByShop = validItems
                .GroupBy(ci => ci.ShopId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 8. FIX: Nếu dto.ShopId được chỉ định, chỉ process shop đó; ngược lại process tất cả
            var shopsToProcess = !Guid.Empty.Equals(dto.ShopId) && itemsByShop.ContainsKey(dto.ShopId)
                ? new Dictionary<Guid, List<CartItem>> { { dto.ShopId, itemsByShop[dto.ShopId] } }
                : itemsByShop;

            if (!shopsToProcess.Any())
            {
                return ServiceResult<OrderDto>.BadRequest(
                    !Guid.Empty.Equals(dto.ShopId)
                        ? $"No items from shop {dto.ShopId} in selected items"
                        : "No valid shops found in items"
                );
            }

            // 9. CREATE ORDER FOR EACH SHOP
            var createdOrders = new List<Order>();

            foreach (var shopGroup in shopsToProcess)
            {
                var shopId = shopGroup.Key;
                var shopItems = shopGroup.Value;

                // Calculate subtotal for this shop
                double subtotalCents = shopItems.Sum(item => item.Price * item.Quantity);

                // ✨ Calculate total weight FOR THIS SHOP (not total for all shops)
                int totalWeightGrams = await CalculateTotalWeightAsync(shopItems);

                // ✨ Calculate shipping fee immediately using ShippingServiceConstants
                int serviceId = ShippingServiceConstants.GetServiceId(dto.ProviderServiceCode);
                string bucketType = ShippingServiceConstants.GetBucketType(totalWeightGrams);
                long baseFee = ShippingServiceConstants.GetBaseFee(serviceId, bucketType);
                long weightSurcharge = (totalWeightGrams / ShippingServiceConstants.WEIGHT_SURCHARGE_UNIT) * ShippingServiceConstants.WEIGHT_SURCHARGE_PER_UNIT;
                long shippingFeeCents = baseFee + weightSurcharge;

                // Total amount = Subtotal + Shipping fee
                double totalAmountCents = subtotalCents + shippingFeeCents;

                // Create Order entity for this shop
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = dto.AccountId,
                    ShopId = shopId,
                    SubtotalCents = subtotalCents,
                    TotalAmountCents = totalAmountCents, // ✨ Includes calculated shipping fee
                    VoucherId = dto.VoucherId,
                    Payment = dto.Payment,
                    Status = OrderStatus.PENDING,
                    DeliveryAddress = dto.DeliveryAddress,
                    CreatedAt = DateTime.UtcNow
                };

                // Create Order Items (snapshot from cart items)
                foreach (var cartItem in shopItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        VersionId = cartItem.VersionId,
                        UnitPriceCents = (long)(cartItem.Price * 100),
                        Quantity = cartItem.Quantity,
                        DiscountCents = 0,
                        TaxCents = 0,
                        LineTotalCents = cartItem.Price * cartItem.Quantity,
                        Status = FulfillmentStatus.UNFULFILLED,
                        CreatedAt = DateTime.UtcNow
                    };

                    order.OrderItems.Add(orderItem);
                }

                // Save order to database
                await _orderRepository.CreateAsync(order);
                createdOrders.Add(order);

                // Publish OrderCreated event to RabbitMQ for ShipmentService + ProductService
                // ✨ TotalAmountCents already includes calculated shipping fee
                // ShipmentService can verify/log the calculation if needed
                _orderEventPublisher.PublishOrderCreated(new OrderCreatedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    AccountId = order.AccountId,
                    DeliveryAddress = order.DeliveryAddress,
                    SubtotalCents = order.SubtotalCents,
                    TotalAmountCents = order.TotalAmountCents,
                    ProviderServiceCode = dto.ProviderServiceCode,
                    TotalWeightGrams = totalWeightGrams,
                    VoucherId = dto.VoucherId,
                    CreatedAt = order.CreatedAt,
                    Items = order.OrderItems.Select(oi => new OrderItemEvent
                    {
                        VersionId = oi.VersionId,
                        Quantity = oi.Quantity
                    }).ToList()
                });
            }

            // 10. DELETE ONLY SELECTED/PROCESSED ITEMS FROM CART
            // FIX: Materialize list to avoid "Collection modified during enumeration" error
            var cartItemsToDelete = cart.CartItems
                .Where(ci => itemsToProcess.Select(ip => ip.CartItemId).Contains(ci.CartItemId))
                .ToList();  // ← IMPORTANT: ToList() materializes before removing

            foreach (var cartItem in cartItemsToDelete)
            {
                await _cartItemRepository.RemoveAsync(cartItem);
            }

            // 11. Return first order (for backward compatibility)
            // In real scenario, return CreateOrderResult with list of all orders
            var orderDto = MapToOrderDto(createdOrders.First());

            return ServiceResult<OrderDto>.Success(orderDto,
                $"Order(s) created successfully. Total orders: {createdOrders.Count}");
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

    public async Task<ServiceResult<List<OrderWithProductDetailsDto>>> GetOrdersByShopIdAsync(Guid shopId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByShopIdAsync(shopId);

            var orderDtos = new List<OrderWithProductDetailsDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapToOrderWithProductDetailsDto(order);
                orderDtos.Add(orderDto);
            }

            return ServiceResult<List<OrderWithProductDetailsDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<OrderWithProductDetailsDto>>.InternalServerError($"Error getting shop orders: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderDto>> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
    {
        try
        {
            // 1. Validate status
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var newStatus))
            {
                return ServiceResult<OrderDto>.BadRequest($"Invalid status: {dto.Status}");
            }

            // 2. Get order
            var order = await _orderRepository.GetOrderWithItemsAsync(dto.OrderId);
            if (order == null)
            {
                return ServiceResult<OrderDto>.NotFound("Order not found");
            }

            // 3. Update status
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // 4. Save changes
            await _orderRepository.UpdateAsync(order);

            // 5. Return updated order
            var orderDto = MapToOrderDto(order);
            return ServiceResult<OrderDto>.Success(orderDto, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error updating order status: {ex.Message}");
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
            ShippingFeeCents = order.TotalAmountCents - order.SubtotalCents, // Tính từ Total - Subtotal
            TotalAmountCents = order.TotalAmountCents,
            VoucherId = order.VoucherId,
            Payment = order.Payment,
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
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

    /// <summary>
    /// Xác định items cần process: selected hoặc toàn bộ (backward compatible)
    /// Nếu không có selected ids → process toàn bộ cart
    /// Nếu có selected ids → chỉ process những items được chọn
    /// </summary>
    private List<CartItem> GetItemsToProcess(
        List<CartItem> allCartItems,
        Guid[]? selectedCartItemIds)
    {
        // Nếu không có selected ids → process toàn bộ (backward compatible)
        if (selectedCartItemIds == null || selectedCartItemIds.Length == 0)
            return allCartItems;

        // Filter chỉ lấy items có id trong list
        var selectedIds = new HashSet<Guid>(selectedCartItemIds);
        return allCartItems
            .Where(ci => selectedIds.Contains(ci.CartItemId))
            .ToList();
    }

    /// <summary>
    /// Calculate total weight of cart items in grams
    /// Truy cập product cache để lấy WeightGrams từ mỗi item
    /// Formula: Sum(ProductCache.WeightGrams * CartItem.Quantity)
    /// </summary>
    private async Task<int> CalculateTotalWeightAsync(List<CartItem> cartItems)
    {
        int totalWeightGrams = 0;

        foreach (var cartItem in cartItems)
        {
            var productCache = await _productCacheRepository.GetByVersionIdAsync(cartItem.VersionId);
            if (productCache != null)
            {
                // Weight per item × Quantity trong cart
                totalWeightGrams += productCache.WeightGrams * cartItem.Quantity;
            }
        }

        return totalWeightGrams;
    }

    /// <summary>
    /// Validate: tất cả selected ids phải nằm trong cart hiện tại
    /// Trả về error message nếu có id không hợp lệ, null nếu ok
    /// </summary>
    private string? ValidateSelectedItemsBelongToCart(
        List<CartItem> cartItems,
        Guid[]? selectedCartItemIds)
    {
        if (selectedCartItemIds == null || selectedCartItemIds.Length == 0)
            return null;

        var cartItemIds = cartItems.Select(ci => ci.CartItemId).ToHashSet();
        var selectedIds = new HashSet<Guid>(selectedCartItemIds);

        // Check if all selected ids exist in cart
        var invalidIds = selectedIds.Except(cartItemIds).ToList();
        if (invalidIds.Any())
        {
            return $"Invalid cart item IDs: {string.Join(", ", invalidIds)}. " +
                   "Item(s) do not belong to user's cart.";
        }

        return null;
    }

    private async Task<OrderWithProductDetailsDto> MapToOrderWithProductDetailsDto(Order order)
    {
        var orderDto = new OrderWithProductDetailsDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            ShopId = order.ShopId,
            SubtotalCents = order.SubtotalCents,
            TotalAmountCents = order.TotalAmountCents,
            VoucherId = order.VoucherId,
            Payment = order.Payment,
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = new List<OrderItemWithProductDetailsDto>()
        };

        // Fetch product details for each order item
        foreach (var orderItem in order.OrderItems)
        {
            var productCache = await _productCacheRepository.GetByVersionIdAsync(orderItem.VersionId);

            var orderItemDto = new OrderItemWithProductDetailsDto
            {
                OrderItemId = orderItem.OrderItemId,
                OrderId = orderItem.OrderId,
                VersionId = orderItem.VersionId,
                UnitPriceCents = orderItem.UnitPriceCents,
                Quantity = orderItem.Quantity,
                DiscountCents = orderItem.DiscountCents,
                TaxCents = orderItem.TaxCents,
                LineTotalCents = orderItem.LineTotalCents,
                ShipmentId = orderItem.ShipmentId,
                Status = orderItem.Status.ToString(),
                CreatedAt = orderItem.CreatedAt,
                // Product details from cache
                ProductName = productCache?.ProductName ?? "Unknown Product",
                ProductDescription = productCache?.ProductDescription,
                SellerSku = productCache?.SellerSku ?? "",
                VersionName = productCache?.VersionName,
                WoodType = productCache?.WoodType,
                WeightGrams = productCache?.WeightGrams ?? 0,
                LengthCm = productCache?.LengthCm ?? 0,
                WidthCm = productCache?.WidthCm ?? 0,
                HeightCm = productCache?.HeightCm ?? 0
            };

            orderDto.OrderItems.Add(orderItemDto);
        }

        return orderDto;
    }
}
