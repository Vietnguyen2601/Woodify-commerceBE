using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho ProviderService Business Service
/// </summary>
public interface IProviderServiceService
{
    Task<ServiceResult<ProviderServiceDto>> CreateAsync(Guid providerId, CreateProviderServiceDto dto);
    Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid serviceId, UpdateProviderServiceDto dto);
    Task<ServiceResult<ProviderServicePagedDto>> GetPagedAsync(GetServicesQueryDto query);
    Task<ServiceResult<ProviderServicePagedDto>> GetByCodeAsync(GetServicesByCodeQueryDto query);
}
