using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ProviderServiceAppService : IProviderServiceService
{
    private const string CacheKeyAllServices = "provider_services:all";
    private static string ProviderServicesCacheKey(Guid providerId) => $"provider_services:{providerId}";

    private readonly IProviderServiceRepository _serviceRepository;
    private readonly IShippingProviderRepository _providerRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMemoryCache _cache;

    public ProviderServiceAppService(
        IProviderServiceRepository serviceRepository,
        IShippingProviderRepository providerRepository,
        IShipmentRepository shipmentRepository,
        IMemoryCache cache)
    {
        _serviceRepository = serviceRepository;
        _providerRepository = providerRepository;
        _shipmentRepository = shipmentRepository;
        _cache = cache;
    }

    public async Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return ServiceResult<IEnumerable<ProviderServiceDto>>.Success(
            services.Select(s => s.ToDto()).ToList());
    }

    public async Task<ServiceResult<IEnumerable<ProviderServiceDto>>> GetByProviderIdAsync(Guid providerId)
    {
        var services = await _serviceRepository.GetByProviderIdAsync(providerId);
        return ServiceResult<IEnumerable<ProviderServiceDto>>.Success(
            services.Select(s => s.ToDto()).ToList());
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
            var service = dto.ToModel(Guid.Empty);
            await _serviceRepository.CreateAsync(service);

            var created = await _serviceRepository.GetByIdAsync(service.ServiceId);

            _cache.Remove(CacheKeyAllServices);

            return ServiceResult<ProviderServiceDto>.Created(
                created!.ToDto(),
                ShipmentMessages.ServiceCreated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError(
                $"{ShipmentMessages.ServiceCreateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError(
                $"{ShipmentMessages.ServiceCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid id, UpdateProviderServiceDto dto)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service is null)
                return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ServiceNotFound);

            bool softDeleting = dto.IsActive.HasValue && !dto.IsActive.Value && service.IsActive;
            if (softDeleting)
            {
                var hasActiveShipments = await _shipmentRepository.HasNonTerminalByServiceIdAsync(id);
                if (hasActiveShipments)
                    return ServiceResult<ProviderServiceDto>.Conflict(ShipmentMessages.ServiceHasActiveShipments);
            }

            dto.MapToUpdate(service);
            await _serviceRepository.UpdateAsync(service);

            _cache.Remove(CacheKeyAllServices);
            _cache.Remove(ProviderServicesCacheKey(service.ProviderId));

            return ServiceResult<ProviderServiceDto>.Success(
                service.ToDto(),
                ShipmentMessages.ServiceUpdated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError(
                $"{ShipmentMessages.ServiceUpdateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ProviderServiceDto>.InternalServerError(
                $"{ShipmentMessages.ServiceUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProviderServiceDto>> GetByShopIdAndCodeAsync(Guid shopId, string code)
    {
        var service = await _serviceRepository.GetByShopIdAndCodeAsync(shopId, code);
        if (service == null)
            return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ServiceNotFound);

        return ServiceResult<ProviderServiceDto>.Success(service.ToDto());
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            if (service is null)
                return ServiceResult.NotFound(ShipmentMessages.ServiceNotFound);

            var hasActiveShipments = await _shipmentRepository.HasNonTerminalByServiceIdAsync(id);
            if (hasActiveShipments)
                return ServiceResult.Conflict(ShipmentMessages.ServiceHasActiveShipments);

            await _serviceRepository.RemoveAsync(service);

            _cache.Remove(CacheKeyAllServices);
            _cache.Remove(ProviderServicesCacheKey(service.ProviderId));

            return ServiceResult.Success(ShipmentMessages.ServiceDeleted);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(
                $"{ShipmentMessages.ServiceDeleteError}: {ex.Message}");
        }
    }
}
