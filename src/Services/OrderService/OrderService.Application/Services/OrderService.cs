using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Helpers;
using OrderService.Application.Shipping;
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
    private readonly IShopInfoCacheRepository _shopInfoCacheRepository;
    private readonly IAccountDirectoryRepository _accountDirectoryRepository;
    private readonly OrderEventPublisher _orderEventPublisher;
    private readonly OrderSideEffectPublisher _orderSideEffects;
    private readonly IOrderRealtimeNotifier? _realtimeNotifier;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IProductVersionCacheRepository productCacheRepository,
        IShopInfoCacheRepository shopInfoCacheRepository,
        IAccountDirectoryRepository accountDirectoryRepository,
        OrderEventPublisher orderEventPublisher,
        OrderSideEffectPublisher orderSideEffects,
        IOrderRealtimeNotifier? realtimeNotifier = null)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productCacheRepository = productCacheRepository;
        _shopInfoCacheRepository = shopInfoCacheRepository;
        _accountDirectoryRepository = accountDirectoryRepository;
        _orderEventPublisher = orderEventPublisher;
        _orderSideEffects = orderSideEffects;
        _realtimeNotifier = realtimeNotifier;
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

                int totalWeightGrams = await CalculateTotalWeightAsync(shopItems);

                decimal commissionRate = CommissionCalculator.DEFAULT_COMMISSION_RATE;

                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = dto.AccountId,
                    ShopId = shopId,
                    CommissionRate = commissionRate,
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

                double merchandiseTotalVnd = order.OrderItems.Sum(oi => oi.LineTotalVnd);
                order.SubtotalVnd = merchandiseTotalVnd;
                long shippingFeeSynced = ShippingPricing.FinalShippingFeeVnd(
                    providerServiceCode,
                    totalWeightGrams,
                    merchandiseTotalVnd);
                order.TotalAmountVnd = merchandiseTotalVnd + shippingFeeSynced;
                order.CommissionVnd = CommissionCalculator.CalculateCommissionVnd(merchandiseTotalVnd, order.CommissionRate);

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
                var totalAmountCents = (long)(order.TotalAmountVnd);
                var commissionCents = (long)(order.CommissionVnd);

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

                _orderSideEffects.PublishOrderSnapshotForProduct(order);
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

            var providerServiceCode = NormalizeProviderServiceCode(request.ProviderServiceCode);

            var orderId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = orderId,
                AccountId = request.AccountId,
                ShopId = request.ShopId,
                VoucherId = request.VoucherId,
                Status = OrderStatus.PENDING,
                DeliveryAddress = request.DeliveryAddress,
                ProviderServiceCode = providerServiceCode,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var cartItem in validItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = orderId,
                    VersionId = cartItem.VersionId,
                    UnitPriceVnd = (long)Math.Round(cartItem.Price),
                    Quantity = cartItem.Quantity,
                    DiscountVnd = 0,
                    TaxVnd = 0,
                    LineTotalVnd = cartItem.Price * cartItem.Quantity,
                    Status = FulfillmentStatus.UNFULFILLED,
                    CreatedAt = DateTime.UtcNow
                });
            }

            double merchandiseTotalVnd = order.OrderItems.Sum(oi => oi.LineTotalVnd);
            order.SubtotalVnd = merchandiseTotalVnd;

            int totalWeightGrams = await CalculateTotalWeightAsync(validItems);

            long shippingFeeVnd = ShippingPricing.FinalShippingFeeVnd(
                providerServiceCode,
                totalWeightGrams,
                merchandiseTotalVnd);

            double totalAmountVnd = merchandiseTotalVnd + shippingFeeVnd;
            order.TotalAmountVnd = totalAmountVnd;

            decimal commissionRate = CommissionCalculator.DEFAULT_COMMISSION_RATE;
            order.CommissionRate = commissionRate;
            long commissionVnd = CommissionCalculator.CalculateCommissionVnd(merchandiseTotalVnd, commissionRate);
            order.CommissionVnd = commissionVnd;

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
            var totalAmountCents = (long)(order.TotalAmountVnd);
            var commissionCents = (long)(order.CommissionVnd);

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

            _orderSideEffects.PublishOrderSnapshotForProduct(order);

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
                SubtotalVnd = merchandiseTotalVnd,
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

            var accountMap = await _accountDirectoryRepository.GetByIdsAsync(new[] { order.AccountId });
            var thumbMap = await LoadThumbnailUrlsByVersionIdAsync(order.OrderItems.Select(i => i.VersionId));
            var orderDto = MapToOrderDto(order, accountMap.GetValueOrDefault(order.AccountId), null, thumbMap);
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
            var accountMap = await _accountDirectoryRepository.GetByIdsAsync(orders.Select(o => o.AccountId));
            var thumbMap = await LoadThumbnailUrlsByVersionIdAsync(orders.SelectMany(o => o.OrderItems.Select(i => i.VersionId)));
            var orderDtos = orders
                .Select(o => MapToOrderDto(o, accountMap.GetValueOrDefault(o.AccountId), null, thumbMap))
                .ToList();

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

            var accountMap = await _accountDirectoryRepository.GetByIdsAsync(orders.Select(o => o.AccountId));
            var orderDtos = new List<OrderWithProductDetailsDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapToOrderWithProductDetailsDto(
                    order,
                    accountMap.GetValueOrDefault(order.AccountId));
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

            // 6–8. Dashboard, ProductService mirror, review eligibility, delivered stock (event-driven)
            _orderSideEffects.PublishAfterStatusChange(order, oldStatus, newStatus.ToString());
            await PublishSellerNetReversalIfNeededAsync(order, oldStatus, newStatus);

            // 9. SignalR realtime (order list by account/shop/order detail)
            if (_realtimeNotifier != null)
            {
                await _realtimeNotifier.NotifyOrderShipmentStatusAsync(new OrderShipmentRealtimePayload
                {
                    ShipmentId = Guid.Empty,
                    OrderId = order.OrderId,
                    ShopId = order.ShopId,
                    AccountId = order.AccountId,
                    ShipmentPreviousStatus = string.Empty,
                    ShipmentNewStatus = string.Empty,
                    OrderPreviousStatus = oldStatus,
                    OrderNewStatus = newStatus.ToString(),
                    OrderRowUpdated = true,
                    OccurredAt = DateTime.UtcNow
                });
            }

            // 10. Return updated order
            var accountMap = await _accountDirectoryRepository.GetByIdsAsync(new[] { order.AccountId });
            var thumbMap = await LoadThumbnailUrlsByVersionIdAsync(order.OrderItems.Select(i => i.VersionId));
            var orderDto = MapToOrderDto(order, accountMap.GetValueOrDefault(order.AccountId), null, thumbMap);
            return ServiceResult<OrderDto>.Success(orderDto, "Order status updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderDto>.InternalServerError($"Error updating order status: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<OrderShipmentRealtimePayload>> ApplyShipmentStatusChangedEventAsync(
        ShipmentStatusChangedEvent evt)
    {
        try
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(evt.OrderId);
            if (order == null)
                return ServiceResult<OrderShipmentRealtimePayload>.NotFound("Order not found");

            var orderPrev = order.Status.ToString();
            var mapped = ShipmentToOrderStatusMapper.MapOrderStatus(evt.NewStatus);

            var payload = new OrderShipmentRealtimePayload
            {
                ShipmentId = evt.ShipmentId,
                OrderId = order.OrderId,
                ShopId = order.ShopId,
                AccountId = evt.AccountId ?? order.AccountId,
                ShipmentPreviousStatus = evt.PreviousStatus,
                ShipmentNewStatus = evt.NewStatus,
                OrderPreviousStatus = orderPrev,
                OrderNewStatus = orderPrev,
                OrderRowUpdated = false,
                OccurredAt = evt.ChangedAt == default ? DateTime.UtcNow : evt.ChangedAt
            };

            if (order.Status is OrderStatus.CANCELLED or OrderStatus.REFUNDED)
            {
                payload.OrderNewStatus = order.Status.ToString();
                return ServiceResult<OrderShipmentRealtimePayload>.Success(payload);
            }

            // Pay flow sets COMPLETED; do not regress to pre-shipping states when shipment is still preparing.
            if (order.Status == OrderStatus.COMPLETED && mapped.HasValue &&
                mapped.Value is OrderStatus.CONFIRMED or OrderStatus.PROCESSING or OrderStatus.READY_TO_SHIP)
            {
                payload.OrderNewStatus = order.Status.ToString();
                return ServiceResult<OrderShipmentRealtimePayload>.Success(payload);
            }

            if (mapped == null || order.Status == mapped)
            {
                payload.OrderNewStatus = order.Status.ToString();
                return ServiceResult<OrderShipmentRealtimePayload>.Success(payload);
            }

            var oldStatus = order.Status.ToString();
            order.Status = mapped.Value;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            _orderSideEffects.PublishAfterStatusChange(order, oldStatus, mapped.Value.ToString());
            await PublishSellerNetReversalIfNeededAsync(order, oldStatus, mapped.Value);

            payload.OrderPreviousStatus = oldStatus;
            payload.OrderNewStatus = mapped.Value.ToString();
            payload.OrderRowUpdated = true;
            return ServiceResult<OrderShipmentRealtimePayload>.Success(payload);
        }
        catch (Exception ex)
        {
            return ServiceResult<OrderShipmentRealtimePayload>.InternalServerError(
                $"Shipment sync error: {ex.Message}");
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

            var accountMap = await _accountDirectoryRepository.GetByIdsAsync(items.Select(o => o.AccountId));
            var shopNameMap = new Dictionary<Guid, string?>();
            foreach (var shopId in items.Select(o => o.ShopId).Distinct())
            {
                var shop = await _shopInfoCacheRepository.GetByShopIdAsync(shopId);
                shopNameMap[shopId] = shop?.Name;
            }

            var thumbMap = await LoadThumbnailUrlsByVersionIdAsync(items.SelectMany(o => o.OrderItems.Select(i => i.VersionId)));
            var result = new OrderListResultDto
            {
                Items = items.Select(o =>
                        MapToOrderDto(
                            o,
                            accountMap.GetValueOrDefault(o.AccountId),
                            shopNameMap.GetValueOrDefault(o.ShopId),
                            thumbMap))
                    .ToList(),
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

    private static void ApplyAccountDirectoryToOrderDto(OrderDto dto, AccountDirectoryEntry? account)
    {
        if (account == null)
            return;
        dto.AccountName = string.IsNullOrWhiteSpace(account.Name) ? null : account.Name.Trim();
        dto.AccountEmail = string.IsNullOrWhiteSpace(account.Email) ? null : account.Email.Trim();
    }

    private OrderDto MapToOrderDto(
        Order order,
        AccountDirectoryEntry? account = null,
        string? shopName = null,
        IReadOnlyDictionary<Guid, string?>? thumbnailsByVersionId = null)
    {
        var thumbMap = thumbnailsByVersionId ?? new Dictionary<Guid, string?>();
        var dto = new OrderDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            AccountName = null,
            AccountEmail = null,
            ShopId = order.ShopId,
            ShopName = shopName,
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
                CreatedAt = oi.CreatedAt,
                ThumbnailUrl = thumbMap.GetValueOrDefault(oi.VersionId)
            }).ToList()
        };
        ApplyAccountDirectoryToOrderDto(dto, account);
        return dto;
    }

    private async Task<IReadOnlyDictionary<Guid, string?>> LoadThumbnailUrlsByVersionIdAsync(
        IEnumerable<Guid> versionIds)
    {
        var ids = versionIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string?>();

        var rows = await _productCacheRepository.GetByVersionIdsAsync(ids);
        return rows.ToDictionary(r => r.VersionId, r => r.ThumbnailUrl);
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

    private async Task<OrderWithProductDetailsDto> MapToOrderWithProductDetailsDto(
        Order order,
        AccountDirectoryEntry? account = null)
    {
        account ??= (await _accountDirectoryRepository.GetByIdsAsync(new[] { order.AccountId }))
            .GetValueOrDefault(order.AccountId);
        var orderDto = new OrderWithProductDetailsDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            AccountName = string.IsNullOrWhiteSpace(account?.Name) ? null : account!.Name.Trim(),
            AccountEmail = string.IsNullOrWhiteSpace(account?.Email) ? null : account!.Email.Trim(),
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

        var cacheRows = await _productCacheRepository.GetByVersionIdsAsync(
            order.OrderItems.Select(i => i.VersionId).ToList());
        var cacheByVersionId = cacheRows.ToDictionary(c => c.VersionId);

        foreach (var orderItem in order.OrderItems)
        {
            cacheByVersionId.TryGetValue(orderItem.VersionId, out var productCache);

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
                HeightCm = productCache?.HeightCm ?? 0,
                ThumbnailUrl = productCache?.ThumbnailUrl
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
        var account = (await _accountDirectoryRepository.GetByIdsAsync(new[] { order.AccountId }))
            .GetValueOrDefault(order.AccountId);
        var dto = new CustomerAccountOrderDto
        {
            OrderId = order.OrderId,
            AccountId = order.AccountId,
            AccountName = string.IsNullOrWhiteSpace(account?.Name) ? null : account!.Name.Trim(),
            AccountEmail = string.IsNullOrWhiteSpace(account?.Email) ? null : account!.Email.Trim(),
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

        var cacheRows = await _productCacheRepository.GetByVersionIdsAsync(
            order.OrderItems.Select(i => i.VersionId).ToList());
        var cacheByVersionId = cacheRows.ToDictionary(c => c.VersionId);

        foreach (var orderItem in order.OrderItems)
        {
            cacheByVersionId.TryGetValue(orderItem.VersionId, out var productCache);
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
                HeightCm = productCache?.HeightCm ?? 0,
                ThumbnailUrl = productCache?.ThumbnailUrl
            });
        }

        return dto;
    }

    public async Task<ServiceResult<List<TopSellingProductAnalyticsDto>>> GetTopSellingProductsAsync(
        int limit = 5,
        Guid? shopId = null)
    {
        const int maxTopSelling = 20;
        limit = Math.Clamp(limit, 1, maxTopSelling);
        var rows = await _orderRepository.GetTopSellingProductMasterAggregatesAsync(limit, shopId);
        if (rows.Count == 0)
            return ServiceResult<List<TopSellingProductAnalyticsDto>>.Success(new List<TopSellingProductAnalyticsDto>(), "No sales data yet");

        var productIds = rows.ConvertAll(r => r.ProductId);
        var caches = await _productCacheRepository.GetActiveByProductIdsAsync(productIds);
        var representativeByProduct = caches
            .GroupBy(c => c.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.LastUpdated).First());

        var shopIds = representativeByProduct.Values.Select(c => c.ShopId).Distinct().ToList();
        var shopNames = new Dictionary<Guid, string>();
        foreach (var sid in shopIds)
        {
            var shop = await _shopInfoCacheRepository.GetByShopIdAsync(sid);
            if (shop != null)
                shopNames[sid] = shop.Name;
        }

        var list = new List<TopSellingProductAnalyticsDto>();
        var rank = 1;
        foreach (var row in rows)
        {
            representativeByProduct.TryGetValue(row.ProductId, out var cache);
            var shopIdRow = cache?.ShopId ?? Guid.Empty;
            list.Add(new TopSellingProductAnalyticsDto
            {
                Rank = rank++,
                ProductId = row.ProductId,
                UnitsSold = row.UnitsSold,
                ProductName = cache?.ProductName ?? string.Empty,
                ProductStatus = cache?.ProductStatus,
                ThumbnailUrl = cache?.ThumbnailUrl,
                VersionId = cache?.VersionId,
                SellerSku = cache?.SellerSku,
                ShopId = shopIdRow,
                ShopName = shopIdRow != Guid.Empty ? shopNames.GetValueOrDefault(shopIdRow) : null
            });
        }

        return ServiceResult<List<TopSellingProductAnalyticsDto>>.Success(list);
    }

    private async Task PublishSellerNetReversalIfNeededAsync(Order order, string oldStatus, OrderStatus newStatus)
    {
        if (!string.Equals(oldStatus, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            return;
        if (newStatus != OrderStatus.CANCELLED && newStatus != OrderStatus.REFUNDED)
            return;

        var shop = await _shopInfoCacheRepository.GetByShopIdAsync(order.ShopId);
        if (shop == null || shop.OwnerAccountId == Guid.Empty)
            return;

        var net = (long)Math.Max(0, order.TotalAmountVnd - order.CommissionVnd);
        if (net <= 0)
            return;

        _orderEventPublisher.PublishOrderSellerNetReversed(new OrderSellerNetReversedEvent
        {
            OrderId = order.OrderId,
            ShopId = order.ShopId,
            SellerAccountId = shop.OwnerAccountId,
            NetAmountVnd = net,
            OccurredAt = DateTime.UtcNow,
            IdempotencyKey = $"order_net_rev:{order.OrderId}"
        });
    }
}
