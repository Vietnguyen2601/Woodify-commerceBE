using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Application.Validators;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShipmentAppService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IShippingProviderRepository _providerRepository;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly IShippingFeeCalculator _feeCalculator;

    public ShipmentAppService(
        IShipmentRepository shipmentRepository,
        IProviderServiceRepository providerServiceRepository,
        IShippingProviderRepository providerRepository,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache,
        IShippingFeeCalculator feeCalculator)
    {
        _shipmentRepository = shipmentRepository;
        _providerServiceRepository = providerServiceRepository;
        _providerRepository = providerRepository;
        _orderInfoCache = orderInfoCache;
        _shopInfoCache = shopInfoCache;
        _feeCalculator = feeCalculator;
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

            var orderInfo = await _orderInfoCache.GetOrderInfoAsync(dto.OrderId);
            if (orderInfo == null)
                return ServiceResult<ShipmentDto>.BadRequest(
                    "Order chưa có trong ShipmentService (cần event order.created qua RabbitMQ).");

            if (orderInfo.ShopId != dto.ShopId)
                return ServiceResult<ShipmentDto>.BadRequest(
                    "order_id does not belong to the given shop_id.");

            var shopInfo = await _shopInfoCache.GetShopInfoAsync(dto.ShopId);

            string providerServiceCode = !string.IsNullOrEmpty(dto.ProviderServiceCode)
                ? dto.ProviderServiceCode
                : orderInfo.ProviderServiceCode
                  ?? shopInfo?.DefaultProviderServiceCode
                  ?? "STD";

            Guid? preferredProviderId = shopInfo?.DefaultProvider;

            // Query for ProviderService with proper provider filtering
            ProviderService? providerService;
            if (preferredProviderId.HasValue)
            {
                // Use shop's default provider for accurate fee calculation
                providerService = await _providerServiceRepository
                    .GetByProviderIdAndCodeAsync(preferredProviderId.Value, providerServiceCode);

                if (providerService == null)
                    return ServiceResult<ShipmentDto>.BadRequest(
                        $"Provider service '{providerServiceCode}' not found for shop's default provider.");
            }
            else
            {
                // Fallback: Query by code only (backward compatibility)
                providerService = await _providerServiceRepository.GetByCodeAsync(providerServiceCode);

                if (providerService == null)
                    return ServiceResult<ShipmentDto>.BadRequest(
                        $"Provider service with code '{providerServiceCode}' not found. Shop may need to configure default provider.");
            }

            double weight = orderInfo.TotalWeightGrams > 0 ? orderInfo.TotalWeightGrams : 1000.0;
            int weightGrams = (int)Math.Ceiling(weight);
            string bulkyType = CalculateBulkyType(weight);

            // Calculate base fee using mock calculator
            int serviceId = _feeCalculator.MapServiceCode(providerServiceCode);
            var feeResult = await _feeCalculator.CalculateAsync(serviceId, weightGrams);

            long baseFee = feeResult.Total;

            // Apply bulky surcharge
            const double BulkySurchargeRate = 0.20;
            const double SuperBulkySurchargeRate = 0.50;
            long surcharge = bulkyType switch
            {
                "BULKY" => (long)Math.Round(baseFee * BulkySurchargeRate),
                "SUPER_BULKY" => (long)Math.Round(baseFee * SuperBulkySurchargeRate),
                _ => 0L
            };

            // Apply provider multiplier
            double multiplier = providerService.MultiplierFee ?? 1.0;
            long finalShippingFeeCents = (long)Math.Round((baseFee + surcharge) * multiplier);

            // Simple free shipping check based on order total
            long orderTotalCents = (long)orderInfo.TotalAmountCents;
            const long FreeShipThreshold = 50000000; // 500k VND = 50M cents
            bool isFreeShipping = orderTotalCents >= FreeShipThreshold;
            if (isFreeShipping)
                finalShippingFeeCents = 0;

            // Use addresses from DTO or fallback to cached values
            string pickupAddress = dto.PickupAddress ?? shopInfo?.DefaultPickupAddress ?? "default_pickup";
            string deliveryAddress = dto.DeliveryAddress ?? orderInfo.DeliveryAddress ?? "default_delivery";

            // Calculate delivery estimated date based on provider service
            DateTime? deliveryEstimatedAt = CalculateDeliveryEstimate(
                orderInfo.CreatedAt,
                providerService.EstimatedDaysMin,
                providerService.EstimatedDaysMax);

            // Create shipment
            var shipment = new Shipment
            {
                ShipmentId = Guid.NewGuid(),
                OrderId = dto.OrderId,
                ProviderServiceId = providerService.ServiceId,
                PickupAddressId = pickupAddress,
                DeliveryAddressId = deliveryAddress,
                Status = "PENDING",
                TrackingNumber = GenerateTrackingNumber(),
                TotalWeightGrams = weight,
                BulkyType = bulkyType,
                FinalShippingFeeCents = finalShippingFeeCents,
                IsFreeShipping = isFreeShipping,
                DeliveryEstimatedAt = deliveryEstimatedAt,
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

            shipment.DeliveryAddressId = dto.DeliveryAddress ?? shipment.DeliveryAddressId;
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

    private string CalculateBulkyType(double weightGrams)
    {
        if (weightGrams >= 5000.0) // >= 5kg
            return "SUPER_BULKY";
        if (weightGrams >= 2000.0) // >= 2kg
            return "BULKY";
        return "NORMAL";
    }

    private DateTime? CalculateDeliveryEstimate(DateTime orderCreatedAt, int? estimatedDaysMin, int? estimatedDaysMax)
    {
        if (!estimatedDaysMax.HasValue)
            return null;

        // Use the maximum estimated days for a conservative estimate
        return orderCreatedAt.AddDays(estimatedDaysMax.Value);
    }

    private string GenerateTrackingNumber()
    {
        return $"TRK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}
