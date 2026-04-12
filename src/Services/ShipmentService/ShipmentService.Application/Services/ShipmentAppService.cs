using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Application.Shipping;
using ShipmentService.Application.Validators;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Constants;
using Shared.Results;
using Shared.Shipping;

namespace ShipmentService.Application.Services;

public class ShipmentAppService : IShipmentService
{
    private static readonly string[] TerminalShipmentStatuses =
    [
        "DELIVERED", "RETURNED", "CANCELLED", "DELIVERY_FAILED"
    ];

    private readonly IShipmentRepository _shipmentRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IShippingProviderRepository _providerRepository;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;

    public ShipmentAppService(
        IShipmentRepository shipmentRepository,
        IProviderServiceRepository providerServiceRepository,
        IShippingProviderRepository providerRepository,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache)
    {
        _shipmentRepository = shipmentRepository;
        _providerServiceRepository = providerServiceRepository;
        _providerRepository = providerRepository;
        _orderInfoCache = orderInfoCache;
        _shopInfoCache = shopInfoCache;
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

    public async Task<ServiceResult<IEnumerable<ShipmentDto>>> GetByShopIdAsync(Guid shopId, string? status = null)
    {
        if (shopId == Guid.Empty)
            return ServiceResult<IEnumerable<ShipmentDto>>.BadRequest("ShopId is required");

        var shipments = await _shipmentRepository.GetByShopIdAsync(shopId, status);
        return ServiceResult<IEnumerable<ShipmentDto>>.Success(
            shipments.Select(s => s.ToDto()).AsEnumerable());
    }

    public async Task<ServiceResult<ShipmentDto>> CreateAsync(CreateShipmentDto dto)
    {
        try
        {
            var validator = new CreateShipmentValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return ServiceResult<ShipmentDto>.BadRequest(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var orderInfo = await _orderInfoCache.GetOrderInfoAsync(dto.OrderId);
            if (orderInfo == null)
                return ServiceResult<ShipmentDto>.BadRequest(
                    "Order is not available in ShipmentService yet (requires order.created via RabbitMQ).");

            if (orderInfo.ShopId != dto.ShopId)
                return ServiceResult<ShipmentDto>.BadRequest(
                    "orderId does not belong to the given shopId.");

            var existing = await _shipmentRepository.GetByOrderIdAsync(dto.OrderId);
            if (existing.Any(s => !TerminalShipmentStatuses.Contains(s.Status)))
                return ServiceResult<ShipmentDto>.Conflict(
                    "An active shipment already exists for this order. Update it or cancel it first.");

            var shopInfo = await _shopInfoCache.GetShopInfoAsync(dto.ShopId);

            string providerServiceCode = !string.IsNullOrEmpty(dto.ProviderServiceCode)
                ? dto.ProviderServiceCode
                : orderInfo.ProviderServiceCode
                  ?? shopInfo?.DefaultProviderServiceCode
                  ?? "STD";

            Guid? preferredProviderId = shopInfo?.DefaultProvider;

            ProviderService? providerService;
            if (preferredProviderId.HasValue)
            {
                providerService = await _providerServiceRepository
                    .GetByProviderIdAndCodeAsync(preferredProviderId.Value, providerServiceCode);

                if (providerService == null)
                    return ServiceResult<ShipmentDto>.BadRequest(
                        $"Provider service '{providerServiceCode}' not found for shop's default provider.");
            }
            else
            {
                providerService = await _providerServiceRepository.GetByCodeAsync(providerServiceCode);

                if (providerService == null)
                    return ServiceResult<ShipmentDto>.BadRequest(
                        $"Provider service with code '{providerServiceCode}' not found. Shop may need to configure default provider.");
            }

            double weight = dto.TotalWeightGrams.HasValue && dto.TotalWeightGrams.Value > 0
                ? dto.TotalWeightGrams.Value
                : (orderInfo.TotalWeightGrams > 0 ? orderInfo.TotalWeightGrams : 1000.0);
            int weightGrams = (int)Math.Ceiling(weight);

            string bulkyType = !string.IsNullOrEmpty(dto.BulkyType)
                ? dto.BulkyType
                : CalculateBulkyType(weight);

            double orderSubtotalVnd = orderInfo.SubtotalVnd > 0 ? orderInfo.SubtotalVnd : 0;
            var canonCode = ShippingServiceConstants.CanonicalizeProviderServiceCode(providerServiceCode);

            long finalShippingFeeVnd;
            bool isFreeShipping;

            if (dto.ForceFreeShipping == true)
            {
                finalShippingFeeVnd = 0;
                isFreeShipping = true;
            }
            else if (dto.FinalShippingFeeVnd.HasValue)
            {
                finalShippingFeeVnd = dto.FinalShippingFeeVnd.Value;
                isFreeShipping = finalShippingFeeVnd == 0;
            }
            else
            {
                finalShippingFeeVnd = ShippingPricing.FinalShippingFeeVnd(
                    canonCode,
                    weightGrams,
                    orderSubtotalVnd);
                isFreeShipping = orderSubtotalVnd >= ShippingPricing.FreeShippingSubtotalThresholdVnd;
            }

            string pickupAddress = dto.PickupAddress ?? shopInfo?.DefaultPickupAddress ?? "default_pickup";
            string deliveryAddress = dto.DeliveryAddress ?? orderInfo.DeliveryAddress ?? "default_delivery";

            DateTime? deliveryEstimatedAt = dto.DeliveryEstimatedAt
                ?? CalculateDeliveryEstimate(
                    orderInfo.CreatedAt,
                    providerService.EstimatedDaysMin,
                    providerService.EstimatedDaysMax);

            string initialStatus = dto.PickupScheduledAt.HasValue ? "PICKUP_SCHEDULED" : "PENDING";

            var shipment = new Shipment
            {
                ShipmentId = Guid.NewGuid(),
                OrderId = dto.OrderId,
                ShopId = dto.ShopId,
                ProviderServiceId = providerService.ServiceId,
                PickupAddressId = pickupAddress,
                DeliveryAddressId = deliveryAddress,
                Status = initialStatus,
                TrackingNumber = GenerateTrackingNumber(),
                TotalWeightGrams = weight,
                BulkyType = bulkyType,
                FinalShippingFeeVnd = finalShippingFeeVnd,
                IsFreeShipping = isFreeShipping,
                PickupScheduledAt = dto.PickupScheduledAt,
                DeliveryEstimatedAt = deliveryEstimatedAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _shipmentRepository.CreateAsync(shipment);

            var created = await _shipmentRepository.GetByIdAsync(shipment.ShipmentId);
            return ServiceResult<ShipmentDto>.Created(
                created!.ToDto(),
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
            var validator = new UpdateShipmentValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return ServiceResult<ShipmentDto>.BadRequest(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            var patchStatus = ShipmentStatusTransitions.Normalize(shipment.Status);
            if (ShipmentStatusTransitions.IsStrictTerminal(patchStatus) ||
                string.Equals(patchStatus, "DELIVERY_FAILED", StringComparison.OrdinalIgnoreCase))
                return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.ShipmentPatchNotAllowedTerminal);

            dto.MapToUpdate(shipment);
            await _shipmentRepository.UpdateAsync(shipment);

            var updated = await _shipmentRepository.GetByIdAsync(id);
            return ServiceResult<ShipmentDto>.Success(
                updated!.ToDto(),
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
            var validator = new UpdateShipmentStatusValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return ServiceResult<ShipmentDto>.BadRequest(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            var newStatus = ShipmentStatusTransitions.Normalize(dto.Status);
            var currentNorm = ShipmentStatusTransitions.Normalize(shipment.Status);

            if (currentNorm == newStatus)
            {
                if (!string.IsNullOrWhiteSpace(dto.FailureReason))
                    shipment.FailureReason = dto.FailureReason.Trim();
                if (!string.IsNullOrWhiteSpace(dto.CancelReason))
                    shipment.CancelReason = dto.CancelReason.Trim();
                shipment.UpdatedAt = DateTime.UtcNow;
                await _shipmentRepository.UpdateAsync(shipment);
                var same = await _shipmentRepository.GetByIdAsync(id);
                return ServiceResult<ShipmentDto>.Success(
                    same!.ToDto(),
                    ShipmentMessages.ShipmentStatusUpdated);
            }

            if (!ShipmentStatusTransitions.TryValidateTransition(shipment.Status, newStatus, out var transitionError))
                return ServiceResult<ShipmentDto>.BadRequest(transitionError ?? ShipmentMessages.ShipmentInvalidTransition);

            if (string.Equals(newStatus, "DELIVERY_FAILED", StringComparison.OrdinalIgnoreCase))
            {
                var fr = dto.FailureReason?.Trim();
                if (string.IsNullOrEmpty(fr) && string.IsNullOrEmpty(shipment.FailureReason?.Trim()))
                    return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.FailureReasonRequired);
                if (!string.IsNullOrEmpty(fr))
                    shipment.FailureReason = fr;
            }

            if (string.Equals(newStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                var cr = dto.CancelReason?.Trim();
                if (string.IsNullOrEmpty(cr) && string.IsNullOrEmpty(shipment.CancelReason?.Trim()))
                    return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.CancelReasonRequired);
                if (!string.IsNullOrEmpty(cr))
                    shipment.CancelReason = cr;
            }

            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            var updated = await _shipmentRepository.GetByIdAsync(id);
            return ServiceResult<ShipmentDto>.Success(
                updated!.ToDto(),
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
            var validator = new UpdateShipmentPickupValidator();
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return ServiceResult<ShipmentDto>.BadRequest(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            var st = ShipmentStatusTransitions.Normalize(shipment.Status);
            if (ShipmentStatusTransitions.IsStrictTerminal(st) ||
                string.Equals(st, "DELIVERY_FAILED", StringComparison.OrdinalIgnoreCase))
                return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.ShipmentPickupNotAllowed);

            if (st is not ("DRAFT" or "PENDING" or "PICKUP_SCHEDULED"))
                return ServiceResult<ShipmentDto>.BadRequest(
                    $"{ShipmentMessages.ShipmentPickupNotAllowed}. Current status: {shipment.Status}.");

            var pickedUpAt = dto.PickedUpAt ?? DateTime.UtcNow;
            shipment.PickedUpAt = pickedUpAt;
            shipment.Status = "PICKED_UP";
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            var updated = await _shipmentRepository.GetByIdAsync(id);
            return ServiceResult<ShipmentDto>.Success(
                updated!.ToDto(),
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

            var delSt = ShipmentStatusTransitions.Normalize(shipment.Status);
            if (!ShipmentStatusTransitions.DeletableStatuses.Contains(delSt))
                return ServiceResult.BadRequest(ShipmentMessages.ShipmentDeleteNotAllowed);

            await _shipmentRepository.RemoveAsync(shipment);

            return ServiceResult.Success(ShipmentMessages.ShipmentDeleted);
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError(
                $"{ShipmentMessages.ShipmentDeleteError}: {ex.Message}");
        }
    }

    private static string CalculateBulkyType(double weightGrams)
    {
        if (weightGrams >= 5000.0)
            return "SUPER_BULKY";
        if (weightGrams >= 2000.0)
            return "BULKY";
        return "NORMAL";
    }

    private static DateTime? CalculateDeliveryEstimate(DateTime orderCreatedAt, int? estimatedDaysMin, int? estimatedDaysMax)
    {
        if (!estimatedDaysMax.HasValue)
            return null;

        return orderCreatedAt.AddDays(estimatedDaysMax.Value);
    }

    private static string GenerateTrackingNumber()
    {
        return $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
    }
}
