using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Helpers;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;
using Shared.Events;
using Shared.Constants;
using Shared.Shipping;

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

    public async Task<ServiceResult<CreateOrdersFromCartResultDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto)
    {
        try
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest("Delivery address is required");

            // 2. Get cart with items
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(dto.AccountId);
            if (cart == null || !cart.CartItems.Any())
            {
                return ServiceResult<CreateOrdersFromCartResultDto>.NotFound("Cart is empty or not found");
            }

            // 3. FILTER ITEMS: Selected or All (BACKWARD COMPATIBLE)
            List<CartItem> itemsToProcess = GetItemsToProcess(
                cart.CartItems.ToList(),
                dto.SelectedCartItemIds
            );

            if (!itemsToProcess.Any())
                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest("No items to process");

            // 4. Validate selected IDs belong to this cart
            var validationError = ValidateSelectedItemsBelongToCart(itemsToProcess, dto.SelectedCartItemIds);
            if (validationError != null)
                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest(validationError);

            // 5. Validate all items comprehensively
            var validationResult = await ValidateCartItemsForCheckoutAsync(itemsToProcess);

            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.InvalidItems.Select(item =>
                    $"Version {item.VersionId}: {item.Reason}").ToList();

                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest(
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
                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest("No valid items to create order");
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
                return ServiceResult<CreateOrdersFromCartResultDto>.BadRequest(
                    !Guid.Empty.Equals(dto.ShopId)
                        ? $"No items from shop {dto.ShopId} in selected items"
                        : "No valid shops found in items"
                );
            }

            // 9. CREATE ORDER FOR EACH SHOP
            var createdOrders = new List<Order>();
            var providerServiceCode = NormalizeProviderServiceCode(dto.ProviderServiceCode);

            foreach (var shopGroup in shopsToProcess)
            {
                var shopId = shopGroup.Key;
                var shopItems = shopGroup.Value;

                // Calculate subtotal for this shop
                double subtotalVnd = shopItems.Sum(item => item.Price * item.Quantity);

                // ✨ Calculate total weight FOR THIS SHOP (not total for all shops)
                int totalWeightGrams = await CalculateTotalWeightAsync(shopItems);

                // ✨ Calculate shipping fee immediately using ShippingServiceConstants
                long shippingFeeVnd = ShippingPricing.FinalShippingFeeVnd(
                    providerServiceCode,
                    totalWeightGrams,
                    subtotalVnd);

                // Total amount = Subtotal + Shipping fee
                double orderTotalAmountVnd = subtotalVnd + shippingFeeVnd;

                // === THÊM MỚI: Tính tiền hoa hồng ===
                // Sử dụng default commission rate (6%) vì không fetch từ shop
                decimal commissionRate = CommissionCalculator.DEFAULT_COMMISSION_RATE;
                long commissionVnd = CommissionCalculator.CalculateCommissionVnd(subtotalVnd, commissionRate);

                // Create Order entity for this shop
                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = dto.AccountId,
                    ShopId = shopId,
                    SubtotalVnd = subtotalVnd,
                    TotalAmountVnd = orderTotalAmountVnd, // ✨ Includes calculated shipping fee
                    CommissionRate = commissionRate,     // === THÊM: Commission rate (6%)
                    CommissionVnd = commissionVnd,   // === THÊM: Tính hoa hồng
                    VoucherId = dto.VoucherId,
                    Status = OrderStatus.PENDING,
                    DeliveryAddress = dto.DeliveryAddress,
                    ProviderServiceCode = providerServiceCode,
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
                        UnitPriceVnd = (long)Math.Round(cartItem.Price),
                        Quantity = cartItem.Quantity,
                        DiscountVnd = 0,
                        TaxVnd = 0,
                        LineTotalVnd = cartItem.Price * cartItem.Quantity,
                        Status = FulfillmentStatus.UNFULFILLED,
                        CreatedAt = DateTime.UtcNow
                    };

                    order.OrderItems.Add(orderItem);
                }

                // Save order to database
                await _orderRepository.CreateAsync(order);
                createdOrders.Add(order);

                // Publish OrderCreated event to RabbitMQ for ShipmentService
                // ✨ TotalAmountVnd already includes calculated shipping fee
                // ShipmentService can verify/log the calculation if needed
                _orderEventPublisher.PublishOrderCreated(new OrderCreatedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    AccountId = order.AccountId,
                    DeliveryAddress = order.DeliveryAddress,
                    SubtotalVnd = order.SubtotalVnd,
                    TotalAmountVnd = order.TotalAmountVnd,
                    ProviderServiceCode = order.ProviderServiceCode,
                    TotalWeightGrams = totalWeightGrams,
                    VoucherId = dto.VoucherId,
                    CreatedAt = order.CreatedAt
                });

                // Publish OrderCreated event to RabbitMQ for ShopService
                var itemCount = order.OrderItems.Count;
                var mainProduct = order.OrderItems.FirstOrDefault();
                var totalAmountCents = (long)(order.TotalAmountVnd * 10);
                var commissionCents = (long)(order.CommissionVnd * 10);

                _orderEventPublisher.PublishOrderCreatedForShop(new OrderCreatedForShopEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    AccountId = order.AccountId,
                    TotalAmountCents = totalAmountCents,
                    CommissionCents = commissionCents,
                    CommissionRate = order.CommissionRate,
                    ItemCount = itemCount,
                    ProductVersionId = mainProduct?.VersionId,
                    ProductVersionName = null,
                    CategoryId = null,
                    CategoryName = null,
                    DeliveryAddress = order.DeliveryAddress,
                    ProviderServiceCode = order.ProviderServiceCode,
                    CreatedAt = order.CreatedAt
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

            // 11. === BUILD RESULT: Danh sách orderIds + tổng tiền ===
            var orderIds = createdOrders.Select(o => o.OrderId).ToList();
            var totalAmountVnd = createdOrders.Sum(o => (long)o.TotalAmountVnd);

            var orderSummaries = createdOrders.Select(o => new OrderSummaryDto
            {
                OrderId = o.OrderId,
                ShopId = o.ShopId,
                SubtotalVnd = o.SubtotalVnd,
                TotalAmountVnd = o.TotalAmountVnd,
                CommissionVnd = o.CommissionVnd,
                ItemCount = o.OrderItems.Count,
                ProviderServiceCode = o.ProviderServiceCode
            }).ToList();

            var result = new CreateOrdersFromCartResultDto
            {
                OrderIds = orderIds,
                TotalAmountVnd = totalAmountVnd,
                OrderCount = createdOrders.Count,
                Orders = orderSummaries
            };

            return ServiceResult<CreateOrdersFromCartResultDto>.Success(result,
                $"Order(s) created successfully. Total orders: {createdOrders.Count}");
        }
        catch (Exception ex)
        {
            return ServiceResult<CreateOrdersFromCartResultDto>.InternalServerError($"Error creating order: {ex.Message}");
        }
    }

    /// <summary>
    /// Create Order từ 1 shop (v2 refactored) - Frontend gọi N lần cho N shops
    /// 
    /// CRITICAL IMPROVEMENTS:
    /// 1. ✅ Only process 1 shop per request (no auto-grouping)
    /// 2. ✅ Explicit shop validation (all items must belong to specified shop)
    /// 3. ✅ Return 1 order object (not list) - simpler integration
    /// 4. ✅ Return ShippingFeeVnd explicitly (for transparency)
    /// 5. ✅ Better error messages for debugging
    /// 
    /// SECURITY NOTES:
    /// - CartItemIds must all belong to specified ShopId
    /// - Items are removed from cart ONLY after successful order creation
    /// - Idempotency: FE should track requests via timestamp or idempotency key (TODO)
    /// </summary>
    public async Task<ServiceResult<CreateOrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            // ===== VALIDATION =====
            if (request.AccountId == Guid.Empty)
                return ServiceResult<CreateOrderResponse>.BadRequest("AccountId is required");

            if (request.ShopId == Guid.Empty)
                return ServiceResult<CreateOrderResponse>.BadRequest("ShopId is required");

            if (request.CartItemIds == null || !request.CartItemIds.Any())
                return ServiceResult<CreateOrderResponse>.BadRequest("At least 1 cart item is required");

            if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
                return ServiceResult<CreateOrderResponse>.BadRequest("DeliveryAddress is required");

            // ===== GET CART =====
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(request.AccountId);
            if (cart == null || !cart.CartItems.Any())
                return ServiceResult<CreateOrderResponse>.NotFound("Cart is empty or not found");

            // ===== FILTER CART ITEMS BY SPECIFIED IDs =====
            var selectedCartItems = cart.CartItems
                .Where(ci => request.CartItemIds.Contains(ci.CartItemId))
                .ToList();

            if (!selectedCartItems.Any())
                return ServiceResult<CreateOrderResponse>.BadRequest("No cart items found with specified IDs");

            // ===== CRITICAL: All items MUST belong to the SAME specified SHOP =====
            var uniqueShops = selectedCartItems.Select(ci => ci.ShopId).Distinct().ToList();
            if (uniqueShops.Count > 1)
                return ServiceResult<CreateOrderResponse>.BadRequest(
                    $"Items belong to multiple shops. Expected shop {request.ShopId} but found items from {uniqueShops.Count} different shops");

            if (uniqueShops.Single() != request.ShopId)
                return ServiceResult<CreateOrderResponse>.BadRequest(
                    $"Items don't belong to shop {request.ShopId}. Items are from shop {uniqueShops.Single()}");

            // ===== VALIDATE ITEMS FOR CHECKOUT =====
            var validationResult = await ValidateCartItemsForCheckoutAsync(selectedCartItems);
            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.InvalidItems.Select(item =>
                    $"Version {item.VersionId}: {item.Reason}").ToList();

                return ServiceResult<CreateOrderResponse>.BadRequest(
                    errorDetails,
                    $"Cannot create order. {validationResult.InvalidItemsCount} of {validationResult.TotalItems} item(s) invalid.");
            }

            // ===== GET VALID ITEMS =====
            var validCartItemIds = selectedCartItems
                .Select(ci => ci.CartItemId)
                .Except(validationResult.InvalidItems.Select(ii => ii.CartItemId))
                .ToHashSet();

            var validItems = selectedCartItems
                .Where(ci => validCartItemIds.Contains(ci.CartItemId))
                .ToList();

            if (!validItems.Any())
                return ServiceResult<CreateOrderResponse>.BadRequest("No valid items to create order");

            // ===== CALCULATE ORDER AMOUNTS =====
            double subtotalVnd = validItems.Sum(item => item.Price * item.Quantity);
            int totalWeightGrams = await CalculateTotalWeightAsync(validItems);

            var providerServiceCode = NormalizeProviderServiceCode(request.ProviderServiceCode);

            long shippingFeeVnd = ShippingPricing.FinalShippingFeeVnd(
                providerServiceCode,
                totalWeightGrams,
                subtotalVnd);

            // Total amount = Subtotal + Shipping fee
            double totalAmountVnd = subtotalVnd + shippingFeeVnd;

            // Calculate commission (6% of subtotal)
            decimal commissionRate = CommissionCalculator.DEFAULT_COMMISSION_RATE;
            long commissionVnd = CommissionCalculator.CalculateCommissionVnd(subtotalVnd, commissionRate);

            // ===== CREATE ORDER =====
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                AccountId = request.AccountId,
                ShopId = request.ShopId,
                SubtotalVnd = subtotalVnd,
                TotalAmountVnd = totalAmountVnd,
                CommissionRate = commissionRate,
                CommissionVnd = commissionVnd,
                VoucherId = request.VoucherId,
                Status = OrderStatus.PENDING,  // ← Stay PENDING until payment succeeds
                DeliveryAddress = request.DeliveryAddress,
                ProviderServiceCode = providerServiceCode,
                CreatedAt = DateTime.UtcNow
            };

            // Create Order Items (snapshot from cart items)
            foreach (var cartItem in validItems)
            {
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    VersionId = cartItem.VersionId,
                    UnitPriceVnd = (long)Math.Round(cartItem.Price),
                    Quantity = cartItem.Quantity,
                    DiscountVnd = 0,
                    TaxVnd = 0,
                    LineTotalVnd = cartItem.Price * cartItem.Quantity,
                    Status = FulfillmentStatus.UNFULFILLED,
                    CreatedAt = DateTime.UtcNow
                };

                order.OrderItems.Add(orderItem);
            }

            // Save order to database
            await _orderRepository.CreateAsync(order);

            // ===== PUBLISH EVENT FOR SHIPMENT SERVICE =====
            _orderEventPublisher.PublishOrderCreated(new OrderCreatedEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                AccountId = order.AccountId,
                DeliveryAddress = order.DeliveryAddress,
                SubtotalVnd = order.SubtotalVnd,
                TotalAmountVnd = order.TotalAmountVnd,
                ProviderServiceCode = order.ProviderServiceCode,
                TotalWeightGrams = totalWeightGrams,
                VoucherId = request.VoucherId,
                CreatedAt = order.CreatedAt
            });

            // ===== PUBLISH EVENT FOR SHOP SERVICE =====
            var itemCount = validItems.Count;
            var mainProduct = order.OrderItems?.FirstOrDefault();
            var totalAmountCents = (long)(order.TotalAmountVnd * 10);
            var commissionCents = (long)(order.CommissionVnd * 10);

            _orderEventPublisher.PublishOrderCreatedForShop(new OrderCreatedForShopEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                AccountId = order.AccountId,
                TotalAmountCents = totalAmountCents,
                CommissionCents = commissionCents,
                CommissionRate = order.CommissionRate,
                ItemCount = itemCount,
                ProductVersionId = mainProduct?.VersionId,
                ProductVersionName = null, // Will be populated by ShopService via ProductService lookup
                CategoryId = null,         // Will be populated by ShopService via ProductService lookup
                CategoryName = null,       // Will be populated by ShopService via ProductService lookup
                DeliveryAddress = order.DeliveryAddress,
                ProviderServiceCode = order.ProviderServiceCode,
                CreatedAt = order.CreatedAt
            });

            // ===== REMOVE SELECTED CART ITEMS =====
            // Important: Remove only items that were processed (not entire cart)
            var cartItemsToDelete = cart.CartItems
                .Where(ci => request.CartItemIds.Contains(ci.CartItemId))
                .ToList();

            foreach (var cartItem in cartItemsToDelete)
            {
                await _cartItemRepository.RemoveAsync(cartItem);
            }

            // ===== BUILD RESPONSE =====
            return ServiceResult<CreateOrderResponse>.Success(new CreateOrderResponse
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                SubtotalVnd = subtotalVnd,
                ShippingFeeVnd = shippingFeeVnd,
                CommissionVnd = commissionVnd,
                TotalAmountVnd = totalAmountVnd,
                ItemCount = validItems.Count,
                Status = "PENDING",
                CreatedAt = order.CreatedAt,
                ProviderServiceCode = order.ProviderServiceCode
            }, $"Order created successfully. Total: {totalAmountVnd} VND");
        }
        catch (Exception ex)
        {
            return ServiceResult<CreateOrderResponse>.InternalServerError($"Error creating order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CheckoutShippingPreviewResponseDto>> PreviewCheckoutShippingAsync(
        CheckoutShippingPreviewRequest request)
    {
        try
        {
            if (request.AccountId == Guid.Empty)
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest("AccountId is required");
            if (request.ShopId == Guid.Empty)
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest("ShopId is required");
            if (request.CartItemIds == null || !request.CartItemIds.Any())
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest("At least 1 cart item is required");

            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(request.AccountId);
            if (cart == null || !cart.CartItems.Any())
                return ServiceResult<CheckoutShippingPreviewResponseDto>.NotFound("Cart is empty or not found");

            var selectedCartItems = cart.CartItems
                .Where(ci => request.CartItemIds.Contains(ci.CartItemId))
                .ToList();

            if (!selectedCartItems.Any())
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest("No cart items found with specified IDs");

            var uniqueShops = selectedCartItems.Select(ci => ci.ShopId).Distinct().ToList();
            if (uniqueShops.Count != 1 || uniqueShops[0] != request.ShopId)
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest(
                    "All items must belong to the specified shop.");

            var validationResult = await ValidateCartItemsForCheckoutAsync(selectedCartItems);
            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.InvalidItems.Select(item =>
                    $"Version {item.VersionId}: {item.Reason}").ToList();
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest(
                    errorDetails,
                    $"Cannot preview. {validationResult.InvalidItemsCount} of {validationResult.TotalItems} item(s) invalid.");
            }

            var validCartItemIds = selectedCartItems
                .Select(ci => ci.CartItemId)
                .Except(validationResult.InvalidItems.Select(ii => ii.CartItemId))
                .ToHashSet();

            var validItems = selectedCartItems
                .Where(ci => validCartItemIds.Contains(ci.CartItemId))
                .ToList();

            if (!validItems.Any())
                return ServiceResult<CheckoutShippingPreviewResponseDto>.BadRequest("No valid items to preview");

            double subtotalVnd = validItems.Sum(item => item.Price * item.Quantity);
            int totalWeightGrams = await CalculateTotalWeightAsync(validItems);

            var quotes = ShippingPricing.QuoteAllCheckoutTiers(totalWeightGrams, subtotalVnd);
            var options = quotes.Select(q => new CheckoutShippingPreviewOptionDto
            {
                ProviderServiceCode = q.ProviderServiceCode,
                DisplayLabel = q.DisplayLabel,
                TotalAmountVnd = q.ShippingFeeVnd,
                IsFreeShipping = q.IsFreeShipping
            }).ToList();

            bool qualifiesFree = subtotalVnd >= ShippingPricing.FreeShippingSubtotalThresholdVnd;

            return ServiceResult<CheckoutShippingPreviewResponseDto>.Success(
                new CheckoutShippingPreviewResponseDto
                {
                    ShopId = request.ShopId,
                    SubtotalVnd = subtotalVnd,
                    TotalWeightGrams = totalWeightGrams,
                    FreeShippingThresholdVnd = ShippingPricing.FreeShippingSubtotalThresholdVnd,
                    SubtotalQualifiesForFreeShipping = qualifiesFree,
                    Options = options
                });
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckoutShippingPreviewResponseDto>.InternalServerError(
                $"Error previewing shipping: {ex.Message}");
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

    public async Task<ServiceResult<List<CustomerAccountOrderDto>>> GetOrdersByAccountForCustomerAsync(Guid accountId)
    {
        try
        {
            var orders = await _orderRepository.GetOrdersByAccountIdAsync(accountId);
            var list = new List<CustomerAccountOrderDto>(orders.Count);
            foreach (var order in orders)
                list.Add(await MapToCustomerAccountOrderDtoAsync(order));
            return ServiceResult<List<CustomerAccountOrderDto>>.Success(list);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<CustomerAccountOrderDto>>.InternalServerError($"Error getting orders: {ex.Message}");
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

            // 3. Store old status for event
            var oldStatus = order.Status.ToString();

            // 4. Update status
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // 5. Save changes
            await _orderRepository.UpdateAsync(order);

            // 6. Publish events for dashboard metrics
            PublishDashboardEvents(order, oldStatus, newStatus.ToString());

            // 7. Return updated order
            var orderDto = MapToOrderDto(order);
            return ServiceResult<OrderDto>.Success(orderDto, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error updating order status: {ex.Message}");
        }
    }

    /// <summary>
    /// Publish events for dashboard metrics when order status changes
    /// </summary>
    private void PublishDashboardEvents(Order order, string oldStatus, string newStatus)
    {
        try
        {
            // Calculate item count from orderItems
            var itemCount = order.OrderItems?.Sum(x => x.Quantity) ?? 0;
            
            // Get first product version as main product (most common scenario)
            var mainProduct = order.OrderItems?.FirstOrDefault();
            var productVersionId = mainProduct?.VersionId;
            
            // Always publish OrderStatusChangedEvent
            var statusChangeEvent = new OrderStatusChangedEvent
            {
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                PreviousStatus = oldStatus,
                NewStatus = newStatus,
                TotalAmountCents = (long)(order.TotalAmountVnd),
                StatusChangedAt = DateTime.UtcNow,
                OrderCreatedAt = order.CreatedAt,
                ItemCount = itemCount,
                ProductVersionId = productVersionId
                // Note: ProductVersionName, CategoryId, CategoryName akan được populated
                // qua ProductService cache lookup ở ShopService consumer layer
            };
            _orderEventPublisher.PublishOrderStatusChanged(statusChangeEvent);

            // Publish OrderCompletedEvent if status became COMPLETED
            if (newStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                var totalAmountCents = (long)(order.TotalAmountVnd);
                var commissionCents = (long)(totalAmountCents * 0.05m);
                
                var completedEvent = new OrderCompletedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    TotalAmountCents = totalAmountCents,
                    CommissionRate = 0.05m, // 5% default commission
                    CommissionCents = commissionCents,
                    CompletedAt = DateTime.UtcNow,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                };
                _orderEventPublisher.PublishOrderCompleted(completedEvent);
            }

            // Publish OrderCancelledEvent if status became CANCELLED
            if (newStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                var cancelledEvent = new OrderCancelledEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    CancelReason = "Order cancelled via dashboard",
                    CancelledAmountCents = (long)(order.TotalAmountVnd),
                    CancelledAt = DateTime.UtcNow,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                };
                _orderEventPublisher.PublishOrderCancelled(cancelledEvent);
            }

            // Publish OrderRefundedEvent if status became REFUNDED/REFUNDING
            if ((newStatus.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) ||
                 newStatus.Equals("REFUNDING", StringComparison.OrdinalIgnoreCase)) &&
                !oldStatus.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("REFUNDING", StringComparison.OrdinalIgnoreCase))
            {
                var refundedEvent = new OrderRefundedEvent
                {
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    RefundAmountCents = (long)(order.TotalAmountVnd),
                    RefundReason = "Order refunded via dashboard",
                    RefundedAt = DateTime.UtcNow,
                    OrderCreatedAt = order.CreatedAt,
                    ItemCount = itemCount,
                    ProductVersionId = productVersionId
                };
                _orderEventPublisher.PublishOrderRefunded(refundedEvent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error publishing dashboard events: {ex.Message}");
            // Don't throw - events are not critical, order update should still succeed
        }
    }

    public async Task<ServiceResult<OrderListResultDto>> GetAllOrdersAsync(GetAllOrdersQueryDto query)
    {
        try
        {
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var (items, total) = await _orderRepository.GetAllPagedAsync(
                page, pageSize, query.Status, query.ShopId, query.AccountId);

            var result = new OrderListResultDto
            {
                Items = items.Select(o => MapToOrderDto(o)).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return ServiceResult<OrderListResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderListResultDto>.InternalServerError($"Error getting orders: {ex.Message}");
        }
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            ShopId = order.ShopId,
            SubtotalVnd = order.SubtotalVnd,
            TotalAmountVnd = order.TotalAmountVnd,
            CommissionRate = order.CommissionRate,      // === Commission rate (6%)
            CommissionVnd = order.CommissionVnd,    // === Commission cents
            VoucherId = order.VoucherId,
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
            ProviderServiceCode = order.ProviderServiceCode,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                VersionId = oi.VersionId,
                UnitPriceVnd = oi.UnitPriceVnd,
                Quantity = oi.Quantity,
                DiscountVnd = oi.DiscountVnd,
                TaxVnd = oi.TaxVnd,
                LineTotalVnd = oi.LineTotalVnd,
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
            SubtotalVnd = order.SubtotalVnd,
            TotalAmountVnd = order.TotalAmountVnd,
            VoucherId = order.VoucherId,
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
            ProviderServiceCode = order.ProviderServiceCode,
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
                UnitPriceVnd = orderItem.UnitPriceVnd,
                Quantity = orderItem.Quantity,
                DiscountVnd = orderItem.DiscountVnd,
                TaxVnd = orderItem.TaxVnd,
                LineTotalVnd = orderItem.LineTotalVnd,
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

    private static string NormalizeProviderServiceCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ShippingServiceConstants.DEFAULT_PROVIDER_SERVICE_CODE;
        var s = ShippingServiceConstants.CanonicalizeProviderServiceCode(code);
        return s.Length <= 20 ? s : s[..20];
    }

    private static string MapPaymentStatusForCustomer(OrderStatus orderStatus) => orderStatus switch
    {
        OrderStatus.PENDING => "PENDING",
        OrderStatus.CANCELLED => "CANCELLED",
        OrderStatus.REFUNDING => "REFUNDING",
        OrderStatus.REFUNDED => "REFUNDED",
        _ => "PAID"
    };

    private async Task<CustomerAccountOrderDto> MapToCustomerAccountOrderDtoAsync(Order order)
    {
        var dto = new CustomerAccountOrderDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            ShopId = order.ShopId,
            SubtotalVnd = order.SubtotalVnd,
            TotalAmountVnd = order.TotalAmountVnd,
            VoucherId = order.VoucherId,
            Status = order.Status.ToString(),
            DeliveryAddress = order.DeliveryAddress,
            ProviderServiceCode = order.ProviderServiceCode,
            PaymentStatus = MapPaymentStatusForCustomer(order.Status),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = new List<CustomerAccountOrderItemDto>()
        };

        foreach (var orderItem in order.OrderItems)
        {
            var productCache = await _productCacheRepository.GetByVersionIdAsync(orderItem.VersionId);
            dto.OrderItems.Add(new CustomerAccountOrderItemDto
            {
                OrderItemId = orderItem.OrderItemId,
                OrderId = orderItem.OrderId,
                VersionId = orderItem.VersionId,
                UnitPriceVnd = orderItem.UnitPriceVnd,
                Quantity = orderItem.Quantity,
                DiscountVnd = orderItem.DiscountVnd,
                TaxVnd = orderItem.TaxVnd,
                LineTotalVnd = orderItem.LineTotalVnd,
                Status = orderItem.Status.ToString(),
                CreatedAt = orderItem.CreatedAt,
                ProductName = productCache?.ProductName ?? "Unknown Product",
                ProductDescription = productCache?.ProductDescription,
                SellerSku = productCache?.SellerSku ?? "",
                VersionName = productCache?.VersionName,
                WoodType = productCache?.WoodType,
                WeightGrams = productCache?.WeightGrams ?? 0,
                LengthCm = productCache?.LengthCm ?? 0,
                WidthCm = productCache?.WidthCm ?? 0,
                HeightCm = productCache?.HeightCm ?? 0
            });
        }

        return dto;
    }

    /// <summary>
    /// Create PayOS payment link và add vào OrderDto
    /// </summary>
}
