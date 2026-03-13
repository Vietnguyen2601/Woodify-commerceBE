using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Application.Validators;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using ShipmentService.Infrastructure.Cache;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShipmentAppService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IShippingProviderRepository _providerRepository;
    private readonly IOrderInfoCacheRepository _orderInfoCache;

    public ShipmentAppService(
        IShipmentRepository shipmentRepository,
        IProviderServiceRepository providerServiceRepository,
        IShippingProviderRepository providerRepository,
        IOrderInfoCacheRepository orderInfoCache)
    {
        _shipmentRepository = shipmentRepository;
        _providerServiceRepository = providerServiceRepository;
        _providerRepository = providerRepository;
        _orderInfoCache = orderInfoCache;
    }

    public async Task<ServiceResult<IEnumerable<ShipmentDto>>> GetAllAsync()
    {
        var shipments = await _shipmentRepository.GetAllAsync();
        return ServiceResult<IEnumerable<ShipmentDto>>.Success(
            shipments.Select(s => s.ToDto()).AsEnumerable());
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
        return ServiceResult<IEnumerable<ShipmentDto>>.Success(
            shipments.Select(s => s.ToDto()).AsEnumerable());
    }

    public async Task<ServiceResult<ShipmentDto>> CreateAsync(CreateShipmentDto dto)
    {
        try
        {
            // Validate input
            var validator = new CreateShipmentValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return ServiceResult<ShipmentDto>.BadRequest(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            // Get order info from cache (RabbitMQ-only flow)
            var cachedOrderInfo = await _orderInfoCache.GetOrderInfoAsync(dto.OrderId);
            if (cachedOrderInfo == null)
                return ServiceResult<ShipmentDto>.BadRequest(
                    "Order not available. Please ensure the order was successfully created and RabbitMQ is properly connected.");

            // Get provider service
            if (string.IsNullOrEmpty(dto.ProviderServiceCode))
                return ServiceResult<ShipmentDto>.BadRequest("ProviderServiceCode is required.");

            var providerService = await _providerServiceRepository.GetByShopIdAndCodeAsync(
                cachedOrderInfo.ShopId, dto.ProviderServiceCode);
            if (providerService == null)
                return ServiceResult<ShipmentDto>.BadRequest(
                    $"Provider service with code '{dto.ProviderServiceCode}' not found.");

            // Calculate shipping fee (using empty items list as placeholder)
            var weight = CalculateWeight(null);

            // Create shipment
            var shipment = new Shipment
            {
                ShipmentId = Guid.NewGuid(),
                OrderId = dto.OrderId,
                ProviderServiceId = providerService.ServiceId,
                DeliveryAddressId = cachedOrderInfo.DeliveryAddressId,
                Status = "PENDING",
                TrackingNumber = GenerateTrackingNumber(),
                TotalWeightGrams = weight,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _shipmentRepository.CreateAsync(shipment);

            return ServiceResult<ShipmentDto>.Created(
                shipment.ToDto(),
                ShipmentMessages.ShipmentCreated);
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<ShipmentDto>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError(
                $"{ShipmentMessages.ShipmentCreateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShipmentDto>> UpdateAsync(Guid id, UpdateShipmentDto dto)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            shipment.DeliveryAddressId = dto.DeliveryAddressId ?? shipment.DeliveryAddressId;
            shipment.TotalWeightGrams = dto.TotalWeightGrams ?? shipment.TotalWeightGrams;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            return ServiceResult<ShipmentDto>.Success(
                shipment.ToDto(),
                ShipmentMessages.ShipmentUpdated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError(
                $"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
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
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            return ServiceResult<ShipmentDto>.Success(
                shipment.ToDto(),
                ShipmentMessages.ShipmentStatusUpdated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError(
                $"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ShipmentDto>> UpdatePickupAsync(Guid id, UpdateShipmentPickupDto dto)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            shipment.UpdatedAt = DateTime.UtcNow;
            await _shipmentRepository.UpdateAsync(shipment);

            return ServiceResult<ShipmentDto>.Success(
                shipment.ToDto(),
                ShipmentMessages.ShipmentUpdated);
        }
        catch (Exception ex)
        {
            return ServiceResult<ShipmentDto>.InternalServerError(
                $"{ShipmentMessages.ShipmentUpdateError}: {ex.Message}");
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
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(
                $"{ShipmentMessages.ShipmentDeleteError}: {ex.Message}");
        }
    }

    private double CalculateWeight(List<OrderItemInfo>? items)
    {
        if (items == null || items.Count == 0)
            return 1.0; // Default 1kg if no items

        return items.Sum(i => (i.Quantity * 0.5)); // Assume 500g per item as placeholder
    }

    private string GenerateTrackingNumber()
    {
        return $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}

public class OrderItemInfo
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
}



