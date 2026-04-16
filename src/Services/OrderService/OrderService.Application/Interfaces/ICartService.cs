using OrderService.Application.DTOs;
using Shared.Results;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface cho Cart Business Service
/// </summary>
public interface ICartService
{
    Task<ServiceResult<CartDto>> GetCartByAccountIdAsync(Guid accountId);
    Task<ServiceResult<CartDto>> AddToCartAsync(Guid accountId, AddToCartDto dto);
    Task<ServiceResult<CartDto>> UpdateCartItemAsync(Guid accountId, UpdateCartItemDto dto);
    Task<ServiceResult> RemoveCartItemAsync(Guid accountId, Guid cartItemId);
    Task<ServiceResult> ClearCartAsync(Guid accountId);
    Task<ServiceResult<CheckoutPreviewDto>> GetCheckoutPreviewAsync(Guid accountId);
}
