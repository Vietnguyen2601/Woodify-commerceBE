using ShopService.Common.DTOs;

namespace ShopService.Services.Interfaces;

public interface IShopService
{
    Task<IEnumerable<ShopDto>> GetAllShopsAsync();
    Task<ShopDto?> GetShopByIdAsync(Guid shopId);
    Task<ShopDto?> GetShopByOwnerIdAsync(Guid ownerId);
    Task<ShopDto> CreateShopAsync(CreateShopDto dto);
    Task<ShopDto?> UpdateShopAsync(Guid shopId, UpdateShopDto dto);
    Task<bool> DeleteShopAsync(Guid shopId);
}
