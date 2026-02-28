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
            Items = items,
            TotalPrice = items.Sum(i => i.TotalPrice),
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
            Price = item.Price,
            TotalPrice = item.Price * item.Quantity
        };
    }
}
