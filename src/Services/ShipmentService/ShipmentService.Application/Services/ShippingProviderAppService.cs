using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShippingProviderAppService : IShippingProviderService
{
    private readonly IShippingProviderRepository _providerRepository;

    public ShippingProviderAppService(IShippingProviderRepository providerRepository)
    {
        _providerRepository = providerRepository;
    }

    public async Task<ServiceResult<IEnumerable<ShippingProviderDto>>> GetAllAsync()
    {
        var providers = await _providerRepository.GetAllAsync();
        return ServiceResult<IEnumerable<ShippingProviderDto>>.Success(providers.Select(p => p.ToDto()));
    }

    public async Task<ServiceResult<ShippingProviderDto>> GetByIdAsync(Guid id)
    {
        var provider = await _providerRepository.GetByIdAsync(id);
        if (provider == null)
            return ServiceResult<ShippingProviderDto>.NotFound(ShipmentMessages.ProviderNotFound);

        return ServiceResult<ShippingProviderDto>.Success(provider.ToDto());
    }

    public async Task<ServiceResult<ShippingProviderDto>> CreateAsync(CreateShippingProviderDto dto)
    {
        try
        {
            var provider = dto.ToModel();
            await _providerRepository.CreateAsync(provider);
            return ServiceResult<ShippingProviderDto>.Created(provider.ToDto(), ShipmentMessages.ProviderCreated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShippingProviderDto>> UpdateAsync(Guid id, UpdateShippingProviderDto dto)
    {
        try
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null)
                return ServiceResult<ShippingProviderDto>.NotFound(ShipmentMessages.ProviderNotFound);

            dto.MapToUpdate(provider);
            await _providerRepository.UpdateAsync(provider);

            var updated = await _providerRepository.GetByIdAsync(id);
            return ServiceResult<ShippingProviderDto>.Success(updated!.ToDto(), ShipmentMessages.ProviderUpdated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var provider = await _providerRepository.GetByIdAsync(id);
            if (provider == null)
                return ServiceResult.NotFound(ShipmentMessages.ProviderNotFound);

            await _providerRepository.RemoveAsync(provider);
            return ServiceResult.Success(ShipmentMessages.ProviderDeleted);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"{ShipmentMessages.ProviderDeleteError}: {ex.Message}");
        }
    }
}
