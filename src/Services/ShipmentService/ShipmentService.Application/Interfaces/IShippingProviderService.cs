using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho ShippingProvider Business Service
/// </summary>
public interface IShippingProviderService
{
    Task<ServiceResult<ShippingProviderDto>> CreateAsync(CreateShippingProviderDto dto);

    Task<ServiceResult<ShippingProviderPagedDto>> GetPagedAsync(GetProvidersQueryDto query);

    Task<ServiceResult<ShippingProviderDto>> GetByIdAsync(Guid providerId);

    Task<ServiceResult<ShippingProviderDto>> UpdateAsync(Guid providerId, UpdateShippingProviderDto dto);

    Task<ServiceResult> DeleteAsync(Guid providerId);
}
