using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappers;

public static class CartMapper
{
    public static CartDto ToDto(this Cart cart)
    {
        if (cart == null) throw new ArgumentNullException(nameof(cart), "Cart cannot be null");

        var items = cart.CartItems?.Select(item => item.ToDto()).ToList() ?? new List<CartItemDto>();
        
        return new CartDto
        {
            CartId = cart.CartId,
            AccountId = cart.AccountId,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            ExpiresAt = cart.ExpiresAt,
            Items = items,
            TotalPriceCents = items.Sum(i => i.TotalPriceCents),
            TotalItems = items.Sum(i => i.Qty)
        };
    }

    public static CartItemDto ToDto(this CartItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item), "CartItem cannot be null");
        
        return new CartItemDto
        {
            CartItemId = item.CartItemId,
            CartId = item.CartId,
            ProductVersionId = item.ProductVersionId,
            SkuCode = item.SkuCode,
            Title = item.Title,
            UnitPriceCents = item.UnitPriceCents,
            Qty = item.Qty,
            TotalPriceCents = item.UnitPriceCents * item.Qty,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
