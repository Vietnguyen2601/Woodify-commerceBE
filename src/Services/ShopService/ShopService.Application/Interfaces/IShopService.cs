using ShopService.Application.DTOs;
using Shared.Results;

namespace ShopService.Application.Interfaces;

/// <summary>
/// Interface cho Shop Business Service
/// </summary>
public interface IShopService
{
    /// <summary>
    /// Lấy tất cả shops ACTIVE (công khai - ẩn bank info)
    /// </summary>
    Task<ServiceResult<IEnumerable<ShopPublicDto>>> GetAllShopsAsync();

    /// <summary>
    /// Lấy tất cả shops (admin - hiển thị bank info)
    /// </summary>
    Task<ServiceResult<IEnumerable<ShopDto>>> GetAllShopsAdminAsync();

    /// <summary>
    /// Lấy shop by ID (công khai - ẩn bank info)
    /// </summary>
    Task<ServiceResult<ShopPublicDto>> GetShopByIdAsync(Guid shopId);

    /// <summary>
    /// Lấy shop by Owner ID (hiển thị bank info)
    /// </summary>
    Task<ServiceResult<ShopDetailDto>> GetShopByOwnerIdAsync(Guid ownerId);

    Task<ServiceResult<RegisterShopResponseDto>> RegisterShopAsync(RegisterShopDto dto);
    Task<ServiceResult<UpdateShopInfoResponseDto>> UpdateShopInfoAsync(Guid shopId, UpdateShopInfoDto dto);
    Task<ServiceResult<UpdateShopStatusResponseDto>> UpdateShopStatusAsync(Guid shopId, UpdateShopStatusDto dto);
    Task<ServiceResult<ShopBankAccountDto>> GetShopBankAccountAsync(Guid shopId);
    Task<ServiceResult<ShopBankAccountDto>> UpdateShopBankAccountAsync(Guid shopId, UpdateShopBankAccountDto dto);
    Task<ServiceResult<ShopDto>> CreateShopAsync(CreateShopDto dto);
    Task<ServiceResult<ShopDto>> UpdateShopAsync(Guid shopId, UpdateShopDto dto);
    Task<ServiceResult> DeleteShopAsync(Guid shopId);
}
