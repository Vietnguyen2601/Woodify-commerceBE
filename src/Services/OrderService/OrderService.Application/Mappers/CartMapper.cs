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
            Items = items,
            TotalPriceCents = items.Sum(i => i.TotalPriceCents),
            TotalItems = items.Sum(i => i.Quantity)
        };
    }

    public static CartItemDto ToDto(this CartItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item), "CartItem cannot be null");
        
        return new CartItemDto
        {
            CartItemId = item.CartItemId,
            CartId = item.CartId,
            VersionId = item.VersionId,
            ShopId = item.ShopId,
            Quantity = item.Quantity,
            UnitPriceCents = item.UnitPriceCents,
            CompareAtPriceCents = item.CompareAtPriceCents,
            IsSelected = item.IsSelected,
            CustomizationNote = item.CustomizationNote,
            IsActive = item.IsActive,
            TotalPriceCents = item.UnitPriceCents * item.Quantity,
            AddedAt = item.AddedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
