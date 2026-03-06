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

    public async Task<ServiceResult<ProviderServiceDto>> CreateAsync(Guid providerId, CreateProviderServiceDto dto)
    {
        try
        {
            var providerExists = await _providerRepository.ExistsAsync(providerId);
            if (!providerExists)
                return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ProviderNotFound);

            var codeExists = await _serviceRepository.ExistsByCodeForProviderAsync(providerId, dto.Code);
            if (codeExists)
                return ServiceResult<ProviderServiceDto>.Conflict(
                    $"A service with code '{dto.Code}' already exists for this provider.");

            var service = dto.ToModel(providerId);
            await _serviceRepository.CreateAsync(service);

            var created = await _serviceRepository.GetByIdAsync(service.ServiceId);

            _cache.Remove(CacheKeyAllServices);
            _cache.Remove(ProviderServicesCacheKey(providerId));

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

    public async Task<ServiceResult<ProviderServiceDto>> UpdateAsync(Guid serviceId, UpdateProviderServiceDto dto)
    {
        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service is null)
                return ServiceResult<ProviderServiceDto>.NotFound(ShipmentMessages.ServiceNotFound);

            bool softDeleting = dto.IsActive.HasValue && dto.IsActive.Value == false && service.IsActive;
            if (softDeleting)
            {
                var hasActiveShipments = await _shipmentRepository.HasNonTerminalByServiceIdAsync(serviceId);
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

    public async Task<ServiceResult<ProviderServicePagedDto>> GetPagedAsync(GetServicesQueryDto query)
    {
        var page = Math.Max(1, query.Page);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var q = _serviceRepository.GetAllQueryable()
            .Include(ps => ps.ShippingProvider)
            .AsQueryable();

        if (query.ProviderId.HasValue)
            q = q.Where(ps => ps.ProviderId == query.ProviderId.Value);

        q = q.OrderBy(ps => ps.ProviderId).ThenBy(ps => ps.Code);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * limit).Take(limit).ToListAsync();

        var result = new ProviderServicePagedDto
        {
            Services = items.Select(ps => ps.ToDto()).ToList(),
            Pagination = new PaginationResultDto { Page = page, Limit = limit, Total = total }
        };

        return ServiceResult<ProviderServicePagedDto>.Success(result);
    }

    public async Task<ServiceResult<ProviderServicePagedDto>> GetByCodeAsync(GetServicesByCodeQueryDto query)
    {
        var page = Math.Max(1, query.Page);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var q = _serviceRepository.GetAllQueryable()
            .Include(ps => ps.ShippingProvider)
            .Where(ps => ps.Code == query.Code.ToUpper())
            .OrderBy(ps => ps.ProviderId)
            .AsQueryable();

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * limit).Take(limit).ToListAsync();

        var result = new ProviderServicePagedDto
        {
            Services = items.Select(ps => ps.ToDto()).ToList(),
            Pagination = new PaginationResultDto { Page = page, Limit = limit, Total = total }
        };

        return ServiceResult<ProviderServicePagedDto>.Success(result);
    }
}
