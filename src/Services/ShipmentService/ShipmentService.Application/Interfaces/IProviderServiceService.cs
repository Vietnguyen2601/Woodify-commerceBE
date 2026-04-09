using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho ProviderService Business Service
/// </summary>
public interface IProviderServiceService
{
    Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetByProviderIdAsync(Guid providerId);
    Task<ServiceResult<ProviderServiceDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ProviderServiceDto>> CreateAsync(CreateProviderServiceDto dto);
    Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid id, UpdateProviderServiceDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<ProviderServiceDto>> GetByShopIdAndCodeAsync(Guid shopId, string code);

    /// <summary>Các dịch vụ (active) của nhà vận chuyển mặc định mà shop đã cấu hình.</summary>
    Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetServicesByShopIdAsync(Guid shopId);
}
