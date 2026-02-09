using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Mappers;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Shared.Results;

namespace OrderService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductVersionCacheRepository _productCacheRepository;

    public CartService(
        ICartRepository cartRepository, 
        ICartItemRepository cartItemRepository,
        IProductVersionCacheRepository productCacheRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productCacheRepository = productCacheRepository;
    }

    public async Task<ServiceResult<CartDto>> GetCartByAccountIdAsync(Guid accountId)
    {
        try
        {
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            
            if (cart == null)
            {
                // Return empty cart if not found
                return ServiceResult<CartDto>.Success(new CartDto
                {
                    AccountId = accountId,
                    Items = new List<CartItemDto>(),
                    TotalPriceCents = 0,
                    TotalItems = 0
                });
            }

            return ServiceResult<CartDto>.Success(cart.ToDto());
        }
        catch (Exception ex)
        {
            return ServiceResult<CartDto>.InternalServerError($"Error getting cart: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CartDto>> AddToCartAsync(Guid accountId, AddToCartDto dto)
    {
        try
        {
            // Validate quantity
            if (dto.Qty <= 0)
                return ServiceResult<CartDto>.BadRequest("Quantity must be greater than 0");

            // Validate product version exists in cache (synchronized from ProductService via events)
            var productCache = await _productCacheRepository.GetByVersionIdAsync(dto.ProductVersionId);
            if (productCache == null)
                return ServiceResult<CartDto>.NotFound("Product version not found or not yet synchronized");

            // Check if product is published
            if (productCache.ProductStatus != "PUBLISHED")
                return ServiceResult<CartDto>.BadRequest($"Product is not available for purchase (Status: {productCache.ProductStatus})");

            // Check if product has price
            if (!productCache.PriceCents.HasValue || productCache.PriceCents.Value <= 0)
                return ServiceResult<CartDto>.BadRequest("Product price is not set");

            // Get or create cart
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            
            if (cart == null)
            {
                // Create new cart
                cart = new Cart
                {
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30) // Cart expires after 30 days
                };
                await _cartRepository.CreateAsync(cart);
            }

            // Check if product already in cart
            var existingItem = await _cartItemRepository.GetByCartIdAndProductVersionIdAsync(cart.CartId, dto.ProductVersionId);
            
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Qty += dto.Qty;
                existingItem.UpdatedAt = DateTime.UtcNow;
                await _cartItemRepository.UpdateAsync(existingItem);
            }
            else
            {
                // Add new cart item (using cached product data)
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductVersionId = dto.ProductVersionId,
                    SkuCode = productCache.Sku ?? "",
                    Title = productCache.Title,
                    UnitPriceCents = productCache.PriceCents.Value,
                    Qty = dto.Qty,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartItemRepository.CreateAsync(cartItem);
            }

            // Update cart timestamp
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            // Reload cart with items
            var updatedCart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            return ServiceResult<CartDto>.Success(updatedCart!.ToDto(), "Product added to cart successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CartDto>.InternalServerError($"Error adding to cart: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CartDto>> UpdateCartItemAsync(Guid accountId, UpdateCartItemDto dto)
    {
        try
        {
            if (dto.Qty <= 0)
                return ServiceResult<CartDto>.BadRequest("Quantity must be greater than 0");

            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            if (cart == null)
                return ServiceResult<CartDto>.NotFound("Cart not found");

            var cartItem = await _cartItemRepository.GetByIdAsync(dto.CartItemId);
            if (cartItem == null || cartItem.CartId != cart.CartId)
                return ServiceResult<CartDto>.NotFound("Cart item not found");

            cartItem.Qty = dto.Qty;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await _cartItemRepository.UpdateAsync(cartItem);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            var updatedCart = await _cartRepository.GetCartWithItemsAsync(cart.CartId);
            return ServiceResult<CartDto>.Success(updatedCart!.ToDto(), "Cart item updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<CartDto>.InternalServerError($"Error updating cart item: {ex.Message}");
        }
    }

    public async Task<ServiceResult> RemoveCartItemAsync(Guid accountId, Guid cartItemId)
    {
        try
        {
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            if (cart == null)
                return ServiceResult.NotFound("Cart not found");

            var cartItem = await _cartItemRepository.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CartId != cart.CartId)
                return ServiceResult.NotFound("Cart item not found");

            await _cartItemRepository.RemoveAsync(cartItem);

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return ServiceResult.Success("Cart item removed successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error removing cart item: {ex.Message}");
        }
    }

    public async Task<ServiceResult> ClearCartAsync(Guid accountId)
    {
        try
        {
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            if (cart == null)
                return ServiceResult.NotFound("Cart not found");

            var items = await _cartItemRepository.GetItemsByCartIdAsync(cart.CartId);
            foreach (var item in items)
            {
                await _cartItemRepository.RemoveAsync(item);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return ServiceResult.Success("Cart cleared successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error clearing cart: {ex.Message}");
        }
    }

    public async Task<ServiceResult<CheckoutPreviewDto>> GetCheckoutPreviewAsync(Guid accountId)
    {
        try
        {
            // Get active cart with items
            var cart = await _cartRepository.GetActiveCartByAccountIdAsync(accountId);
            if (cart == null)
            {
                return ServiceResult<CheckoutPreviewDto>.NotFound("Cart not found or empty");
            }

            var cartItems = await _cartItemRepository.GetItemsByCartIdAsync(cart.CartId);
            if (!cartItems.Any())
            {
                return ServiceResult<CheckoutPreviewDto>.BadRequest("Cart is empty");
            }

            // Get all product version IDs to check validity
            var versionIds = cartItems.Select(item => item.ProductVersionId).ToList();
            var checkoutItems = new List<CheckoutItemDto>();
            long subtotal = 0;
            int validCount = 0;
            int invalidCount = 0;

            foreach (var item in cartItems)
            {
                var checkoutItem = new CheckoutItemDto
                {
                    CartItemId = item.CartItemId,
                    ProductVersionId = item.ProductVersionId,
                    SkuCode = item.SkuCode,
                    Title = item.Title,
                    UnitPriceCents = item.UnitPriceCents,
                    Qty = item.Qty,
                    TotalPriceCents = item.UnitPriceCents * item.Qty,
                    AddedAt = item.CreatedAt,
                    IsValid = true
                };

                // Check if product version still exists and is valid in cache
                var productCache = await _productCacheRepository.GetByVersionIdAsync(item.ProductVersionId);
                
                if (productCache == null)
                {
                    checkoutItem.IsValid = false;
                    checkoutItem.InvalidReason = "Product is no longer available";
                    invalidCount++;
                }
                else if (productCache.ProductStatus == "ARCHIVED" || productCache.ProductStatus == "DELETED")
                {
                    checkoutItem.IsValid = false;
                    checkoutItem.InvalidReason = $"Product is {productCache.ProductStatus.ToLower()}";
                    invalidCount++;
                }
                else if (productCache.ProductStatus != "PUBLISHED")
                {
                    checkoutItem.IsValid = false;
                    checkoutItem.InvalidReason = "Product is not available for purchase";
                    invalidCount++;
                }
                else
                {
                    // Valid item, add to subtotal
                    subtotal += checkoutItem.TotalPriceCents;
                    validCount++;
                }

                checkoutItems.Add(checkoutItem);
            }

            var preview = new CheckoutPreviewDto
            {
                CartId = cart.CartId,
                AccountId = cart.AccountId,
                Items = checkoutItems,
                SubtotalCents = subtotal,
                TotalItems = cartItems.Count,
                ValidItemsCount = validCount,
                InvalidItemsCount = invalidCount,
                HasInvalidItems = invalidCount > 0,
                CreatedAt = cart.CreatedAt
            };

            return ServiceResult<CheckoutPreviewDto>.Success(preview);
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckoutPreviewDto>.InternalServerError($"Error getting checkout preview: {ex.Message}");
        }
    }
}
