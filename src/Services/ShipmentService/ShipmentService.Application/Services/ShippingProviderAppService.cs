using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShippingProviderAppService : IShippingProviderService
{
    // Cache keys
    private const string CacheKeyAllProviders = "providers:all";
    private static string ProviderCacheKey(Guid id) => $"providers:{id}";

    private readonly IShippingProviderRepository _providerRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMemoryCache _cache;

    public ShippingProviderAppService(
        IShippingProviderRepository providerRepository,
        IProviderServiceRepository providerServiceRepository,
        IShipmentRepository shipmentRepository,
        IMemoryCache cache)
    {
        _providerRepository = providerRepository;
        _providerServiceRepository = providerServiceRepository;
        _shipmentRepository = shipmentRepository;
        _cache = cache;
    }

    public async Task<ServiceResult<ShippingProviderDto>> CreateAsync(CreateShippingProviderDto dto)
    {
        try
        {
            var nameExists = await _providerRepository.ExistsByNameAsync(dto.Name);
            if (nameExists)
                return ServiceResult<ShippingProviderDto>.Conflict(ShipmentMessages.ProviderNameDuplicate);

            var provider = dto.ToModel();
            await _providerRepository.CreateAsync(provider);

            _cache.Remove(CacheKeyAllProviders);

            return ServiceResult<ShippingProviderDto>.Created(provider.ToDto(), ShipmentMessages.ProviderCreated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderCreateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShippingProviderPagedDto>> GetPagedAsync(GetProvidersQueryDto query)
    {
        var page = Math.Max(1, query.Page);
        var limit = Math.Clamp(query.Limit, 1, 100);

        var queryable = _providerRepository.GetAllQueryable()
            .OrderBy(p => p.Name);

        var total = await queryable.CountAsync();

        var items = await queryable
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var result = new ShippingProviderPagedDto
        {
            Providers = items.Select(p => p.ToDto()).ToList(),
            Pagination = new PaginationResultDto
            {
                Page = page,
                Limit = limit,
                Total = total
            }
        };

        return ServiceResult<ShippingProviderPagedDto>.Success(result);
    }

    public async Task<ServiceResult<ShippingProviderDto>> UpdateAsync(Guid providerId, UpdateShippingProviderDto dto)
    {
        try
        {
            var provider = await _providerRepository.GetByIdAsync(providerId);
            if (provider is null)
                return ServiceResult<ShippingProviderDto>.NotFound(ShipmentMessages.ProviderNotFound);

            if (dto.Name is not null)
            {
                var nameTaken = await _providerRepository.ExistsByNameExcludingIdAsync(dto.Name, providerId);
                if (nameTaken)
                    return ServiceResult<ShippingProviderDto>.Conflict(ShipmentMessages.ProviderNameDuplicate);
            }

            bool softDeleting = dto.IsActive.HasValue && !dto.IsActive.Value && provider.IsActive;
            if (softDeleting)
            {
                var hasActiveServices = await _providerServiceRepository.HasActiveByProviderIdAsync(providerId);
                if (hasActiveServices)
                    return ServiceResult<ShippingProviderDto>.Conflict(ShipmentMessages.ProviderHasActiveServices);

                var hasActiveShipments = await _shipmentRepository.HasNonTerminalByProviderIdAsync(providerId);
                if (hasActiveShipments)
                    return ServiceResult<ShippingProviderDto>.Conflict(ShipmentMessages.ProviderHasActiveShipments);
            }

            dto.MapToUpdate(provider);
            await _providerRepository.UpdateAsync(provider);

            _cache.Remove(CacheKeyAllProviders);
            _cache.Remove(ProviderCacheKey(providerId));

            var message = softDeleting
                ? ShipmentMessages.ProviderDeleted
                : ShipmentMessages.ProviderUpdated;

            return ServiceResult<ShippingProviderDto>.Success(provider.ToDto(), message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderUpdateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShippingProviderDto>.InternalServerError($"{ShipmentMessages.ProviderUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShippingProviderDto>> GetByIdAsync(Guid providerId)
    {
        var cached = _cache.Get<ShippingProviderDto>(ProviderCacheKey(providerId));
        if (cached is not null)
            return ServiceResult<ShippingProviderDto>.Success(cached);

        var provider = await _providerRepository.GetByIdAsync(providerId);
        if (provider is null)
            return ServiceResult<ShippingProviderDto>.NotFound(ShipmentMessages.ProviderNotFound);

        var dto = provider.ToDto();
        _cache.Set(ProviderCacheKey(providerId), dto, TimeSpan.FromMinutes(5));
        return ServiceResult<ShippingProviderDto>.Success(dto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid providerId)
    {
        try
        {
            var provider = await _providerRepository.GetByIdAsync(providerId);
            if (provider is null)
                return ServiceResult.NotFound(ShipmentMessages.ProviderNotFound);

            var hasActiveServices = await _providerServiceRepository.HasActiveByProviderIdAsync(providerId);
            if (hasActiveServices)
                return ServiceResult.Conflict(ShipmentMessages.ProviderHasActiveServices);

            var hasActiveShipments = await _shipmentRepository.HasNonTerminalByProviderIdAsync(providerId);
            if (hasActiveShipments)
                return ServiceResult.Conflict(ShipmentMessages.ProviderHasActiveShipments);

            await _providerRepository.RemoveAsync(provider);

            _cache.Remove(CacheKeyAllProviders);
            _cache.Remove(ProviderCacheKey(providerId));

            return ServiceResult.Success(ShipmentMessages.ProviderDeleted);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"{ShipmentMessages.ProviderDeleteError}: {ex.Message}");
        }
    }
}
