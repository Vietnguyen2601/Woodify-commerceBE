using ShopService.Application.DTOs;
using Shared.Results;

namespace ShopService.Application.Interfaces;

/// <summary>
/// Interface cho Shop Business Service
/// </summary>
public interface IShopService
{
    Task<ServiceResult<IEnumerable<ShopDto>>> GetAllShopsAsync();
    Task<ServiceResult<ShopDto>> GetShopByIdAsync(Guid shopId);
    Task<ServiceResult<ShopDto>> GetShopByOwnerIdAsync(Guid ownerId);
    Task<ServiceResult<ShopDto>> CreateShopAsync(CreateShopDto dto);
    Task<ServiceResult<ShopDto>> UpdateShopAsync(Guid shopId, UpdateShopDto dto);
    Task<ServiceResult> DeleteShopAsync(Guid shopId);
}
