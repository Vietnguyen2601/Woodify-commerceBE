using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho ProviderService Business Service
/// </summary>
public interface IProviderServiceService
{
<<<<<<< HEAD
    Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetByProviderIdAsync(Guid providerId);
    Task<ServiceResult<ProviderServiceDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ProviderServiceDto>> CreateAsync(CreateProviderServiceDto dto);
    Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid id, UpdateProviderServiceDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
    Task<ServiceResult<ProviderServiceDto>> GetByShopIdAndCodeAsync(Guid shopId, string code);
=======
    Task<ServiceResult<ProviderServiceDto>> CreateAsync(Guid providerId, CreateProviderServiceDto dto);
    Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid serviceId, UpdateProviderServiceDto dto);
    Task<ServiceResult<ProviderServicePagedDto>> GetPagedAsync(GetServicesQueryDto query);
    Task<ServiceResult<ProviderServicePagedDto>> GetByCodeAsync(GetServicesByCodeQueryDto query);
>>>>>>> develop
}
