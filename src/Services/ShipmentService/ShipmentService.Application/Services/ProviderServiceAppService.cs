using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ProviderServiceAppService : IProviderServiceService
{
    private readonly IProviderServiceRepository _serviceRepository;

    public ProviderServiceAppService(IProviderServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return ServiceResult<IEnumerable<ProviderServiceDto>>.Success(services.Select(s => s.ToDto()));
    }

    public async Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetByProviderIdAsync(Guid providerId)
    {
        var services = await _serviceRepository.GetByProviderIdAsync(providerId);
        return ServiceResult<IEnumerable<ProviderServiceDto>>.Success(services.Select(s => s.ToDto()));
    }

    public async Task<ServiceResult<ProviderServiceDto>> GetByIdAsync(Guid id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null)
            return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ServiceNotFound);

        return ServiceResult<ProviderServiceDto>.Success(service.ToDto());
    }

    public async Task<ServiceResult<ProviderServiceDto>> CreateAsync(CreateProviderServiceDto dto)
    {
        try
        {
            var service = dto.ToModel();
            await _serviceRepository.CreateAsync(service);
            return ServiceResult<ProviderServiceDto>.Created(service.ToDto(), ShipmentMessages.ServiceCreated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError($"{ShipmentMessages.ServiceCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid id, UpdateProviderServiceDto dto)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ServiceNotFound);

            dto.MapToUpdate(service);
            await _serviceRepository.UpdateAsync(service);

            var updated = await _serviceRepository.GetByIdAsync(id);
            return ServiceResult<ProviderServiceDto>.Success(updated!.ToDto(), ShipmentMessages.ServiceUpdated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError($"{ShipmentMessages.ServiceUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service == null)
                return ServiceResult.NotFound(ShipmentMessages.ServiceNotFound);

            await _serviceRepository.RemoveAsync(service);
            return ServiceResult.Success(ShipmentMessages.ServiceDeleted);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"{ShipmentMessages.ServiceDeleteError}: {ex.Message}");
        }
    }
}
