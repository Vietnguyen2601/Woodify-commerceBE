using ShipmentService.Application.DTOs;
using Shared.Results;

namespace ShipmentService.Application.Interfaces;

/// <summary>
/// Interface cho ShippingProvider Business Service
/// </summary>
public interface IShippingProviderService
{
    Task<ServiceResult<IEnumerable<ShippingProviderDto>>> GetAllAsync();
    Task<ServiceResult<ShippingProviderDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<ShippingProviderDto>> CreateAsync(CreateShippingProviderDto dto);
    Task<ServiceResult<ShippingProviderDto>> UpdateAsync(Guid id, UpdateShippingProviderDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}
