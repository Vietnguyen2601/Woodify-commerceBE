using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShipmentAppService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;

    public ShipmentAppService(
        IShipmentRepository shipmentRepository,
        IProviderServiceRepository providerServiceRepository)
    {
        _shipmentRepository = shipmentRepository;
        _providerServiceRepository = providerServiceRepository;
    }

    public async Task<ServiceResult<IEnumerable<ShipmentDto>>> GetAllAsync()
    {
        var shipments = await _shipmentRepository.GetAllAsync();
        return ServiceResult<IEnumerable<ShipmentDto>>.Success(shipments.Select(s => s.ToDto()));
    }

    public async Task<ServiceResult<ShipmentDto>> GetByIdAsync(Guid id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null)
            return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

        return ServiceResult<ShipmentDto>.Success(shipment.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<ShipmentDto>>> GetByOrderIdAsync(Guid orderId)
    {
        var shipments = await _shipmentRepository.GetByOrderIdAsync(orderId);
        return ServiceResult<IEnumerable<ShipmentDto>>.Success(shipments.Select(s => s.ToDto()));
    }

    public async Task<ServiceResult<ShipmentDto>> CreateAsync(CreateShipmentDto dto)
    {
        try
        {
            var shipment = dto.ToModel();
            await _shipmentRepository.CreateAsync(shipment);

            if (shipment.ProviderServiceId.HasValue)
                shipment.ProviderService = await _providerServiceRepository.GetByIdAsync(shipment.ProviderServiceId.Value);

            return ServiceResult<ShipmentDto>.Created(shipment.ToDto(), ShipmentMessages.ShipmentCreated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentCreateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShipmentDto>> UpdateAsync(Guid id, UpdateShipmentDto dto)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            dto.MapToUpdate(shipment);
            await _shipmentRepository.UpdateAsync(shipment);

            var updated = await _shipmentRepository.GetByIdAsync(id);
            return ServiceResult<ShipmentDto>.Success(updated!.ToDto(), ShipmentMessages.ShipmentUpdated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShipmentDto>> UpdateStatusAsync(Guid id, UpdateShipmentStatusDto dto)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            shipment.Status = dto.Status;
            shipment.FailureReason = dto.FailureReason ?? shipment.FailureReason;
            shipment.CancelReason = dto.CancelReason ?? shipment.CancelReason;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            var updated = await _shipmentRepository.GetByIdAsync(id);
            return ServiceResult<ShipmentDto>.Success(updated!.ToDto(), ShipmentMessages.ShipmentStatusUpdated);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError($"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult.NotFound(ShipmentMessages.ShipmentNotFound);

            await _shipmentRepository.RemoveAsync(shipment);
            return ServiceResult.Success(ShipmentMessages.ShipmentDeleted);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return ServiceResult.InternalServerError($"{ShipmentMessages.ShipmentDeleteError}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult.InternalServerError($"{ShipmentMessages.ShipmentDeleteError}: {ex.Message}");
        }
    }
}
