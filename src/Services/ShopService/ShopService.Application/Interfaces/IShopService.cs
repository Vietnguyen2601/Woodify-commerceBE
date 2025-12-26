using ShopService.Application.DTOs;

namespace ShopService.Application.Interfaces;

public interface IShopService
{
    Task<IEnumerable<ShopDto>> GetAllShopsAsync();
    Task<ShopDto?> GetShopByIdAsync(Guid shopId);
    Task<ShopDto?> GetShopByOwnerIdAsync(Guid ownerId);
    Task<ShopDto> CreateShopAsync(CreateShopDto dto);
    Task<ShopDto?> UpdateShopAsync(Guid shopId, UpdateShopDto dto);
    Task<bool> DeleteShopAsync(Guid shopId);
}
