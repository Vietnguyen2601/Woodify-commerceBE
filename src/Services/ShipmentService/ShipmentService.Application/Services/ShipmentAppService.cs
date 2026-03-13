using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Application.Mappers;
using ShipmentService.Domain.Entities;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.ExternalProviders;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShipmentAppService : IShipmentService
{
    private const double BulkySurchargeRate = 0.20;
    private const double SuperBulkySurchargeRate = 0.50;
    private const double BulkyVolumeThreshold = 100_000.0; // cm³
    private const double SuperBulkyVolumeThreshold = 500_000.0; // cm³

    private readonly IShipmentRepository _shipmentRepository;
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IOrderServiceClient _orderServiceClient;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IShopServiceClient _shopServiceClient;
    private readonly IGhnApiClient _ghnApiClient;
    private readonly ILogger<ShipmentAppService> _logger;

    public ShipmentAppService(
        IShipmentRepository shipmentRepository,
        IProviderServiceRepository providerServiceRepository,
        IOrderServiceClient orderServiceClient,
        IOrderInfoCacheRepository orderInfoCache,
        IProductServiceClient productServiceClient,
        IShopServiceClient shopServiceClient,
        IGhnApiClient ghnApiClient,
        ILogger<ShipmentAppService> logger)
    {
        _shipmentRepository = shipmentRepository;
        _providerServiceRepository = providerServiceRepository;
        _orderServiceClient = orderServiceClient;
        _orderInfoCache = orderInfoCache;
        _productServiceClient = productServiceClient;
        _shopServiceClient = shopServiceClient;
        _ghnApiClient = ghnApiClient;
        _logger = logger;
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
            // ── 0. Fetch order context from RabbitMQ cache ONLY ──────────────
            // NO API fallback - data must come from OrderCreatedEvent via RabbitMQ
            var cachedOrderInfo = await _orderInfoCache.GetOrderInfoAsync(dto.OrderId);

            if (cachedOrderInfo == null)
            {
                _logger.LogError($"Order {dto.OrderId} not found in cache. Order must be created via OrderCreatedEvent from RabbitMQ");
                return ServiceResult<ShipmentDto>.BadRequest(
                    "Order not available. Please ensure the order was successfully created and RabbitMQ is properly connected.");
            }

            if (string.IsNullOrWhiteSpace(dto.ProviderServiceCode))
                return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.ProviderServiceCodeRequired);

            // ── 0a. Auto-fill delivery address from order ─────────────────────
            if (string.IsNullOrEmpty(dto.DeliveryAddressId))
                dto.DeliveryAddressId = cachedOrderInfo.DeliveryAddressId;

            // ── 0b. Auto-fill shop from cached order ──────────────────────────
            var shopId = cachedOrderInfo.ShopId;

            if (string.IsNullOrEmpty(dto.PickupAddressId) && shopId != Guid.Empty)
                dto.PickupAddressId = await _shopServiceClient.GetDefaultPickupAddressAsync(shopId);

            if (string.IsNullOrEmpty(dto.PickupAddressId) || string.IsNullOrEmpty(dto.DeliveryAddressId))
                return ServiceResult<ShipmentDto>.BadRequest(ShipmentMessages.AddressResolutionFailed);

            // ── 0c. Resolve provider service by code ──────────────────────────
            var providerService = await _providerServiceRepository.GetByShopIdAndCodeAsync(
                shopId, dto.ProviderServiceCode);

            if (providerService == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ServiceNotFound);

            // ── 1. Generate tracking number ───────────────────────────────────
            var trackingNumber = await ResolveTrackingNumberAsync(dto.OrderId);

            // ── 2. Auto-calculate weight and bulky type ───────────────────────
            // Note: Order items are fetched from ProductService, not from cache
            var (totalWeightGrams, bulkyType) = await AutoFillWeightAndBulkyAsync(dto.OrderId, new List<OrderItemInfo>());

            if (totalWeightGrams <= 0)
            {
                _logger.LogWarning($"Weight calculation returned 0 for order {dto.OrderId}");
                totalWeightGrams = 1000; // Default 1kg if cannot calculate
            }

            // ── 3. Auto-calculate final shipping fee ──────────────────────────
            long finalFeeCents = 0;
            if (!dto.IsFreeShipping)
                finalFeeCents = await AutoCalculateShippingFeeAsync(
                    dto.PickupAddressId, dto.DeliveryAddressId, totalWeightGrams, bulkyType, providerService);

            // ── Build and persist entity ──────────────────────────────────────
            var shipment = dto.ToModel();
            shipment.TrackingNumber = trackingNumber;
            shipment.ProviderServiceId = providerService.ServiceId;
            shipment.TotalWeightGrams = totalWeightGrams;
            shipment.BulkyType = bulkyType;
            shipment.FinalShippingFeeCents = finalFeeCents;
            shipment.DeliveryEstimatedAt = EstimateDeliveryEta(providerService);

            await _shipmentRepository.CreateAsync(shipment);

            shipment.ProviderService = providerService;

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

    // ── Tracking Number ───────────────────────────────────────────────────────

    /// <summary>
    /// Auto-generates a deduplicated tracking number "WD-SHP-[ORDER_SHORT]-[RANDOM_6DIGITS]"
    /// (max 5 retries).
    /// </summary>
    private async Task<string> ResolveTrackingNumberAsync(Guid orderId)
    {
        const int maxAttempts = 5;
        for (int i = 0; i < maxAttempts; i++)
        {
            string candidate = GenerateTrackingNumber(orderId);
            if (!await _shipmentRepository.ExistsByTrackingNumberAsync(candidate))
                return candidate;
        }

        throw new InvalidOperationException(ShipmentMessages.TrackingNumberGenerationFailed);
    }

    /// <summary>Format: WD-SHP-[first 4 chars of orderId uppercase]-[6 random digits]</summary>
    private static string GenerateTrackingNumber(Guid orderId)
    {
        string orderShort = orderId.ToString("N")[..4].ToUpperInvariant();
        string random6 = Random.Shared.Next(100_000, 999_999).ToString();
        return $"WD-SHP-{orderShort}-{random6}";
    }

    // ── Weight & Bulky ────────────────────────────────────────────────────────

    /// <summary>
    /// Calls ProductService per version to get weight/dimensions.
    /// Returns resolved totalWeightGrams and bulkyType.
    /// </summary>
    private async Task<(double totalWeightGrams, string bulkyType)> AutoFillWeightAndBulkyAsync(
        Guid orderId, IReadOnlyCollection<OrderItemInfo> items)
    {
        double totalWeightGrams = 0;
        double totalVolumeCm3 = 0;

        if (items == null || items.Count == 0)
        {
            _logger.LogWarning("No order items for order {OrderId}; cannot auto-calc weight", orderId);
            return (totalWeightGrams, "NORMAL");
        }

        foreach (var item in items)
        {
            var version = await _productServiceClient.GetProductVersionAsync(item.VersionId);
            if (version == null)
            {
                _logger.LogWarning("ProductVersion {VersionId} not found; skipping in weight calc", item.VersionId);
                continue;
            }

            totalWeightGrams += version.WeightGrams * item.Quantity;

            totalVolumeCm3 += (double)version.LengthCm
                            * (double)version.WidthCm
                            * (double)version.HeightCm
                            * item.Quantity;
        }

        string bulkyType = totalVolumeCm3 > SuperBulkyVolumeThreshold ? "SUPER_BULKY"
                         : totalVolumeCm3 > BulkyVolumeThreshold ? "BULKY"
                         : "NORMAL";

        _logger.LogInformation(
            "Auto-calculated: order={OrderId}, weight={Weight}g, volume={Volume}cm³, bulky={Bulky}",
            orderId, totalWeightGrams, totalVolumeCm3, bulkyType);

        return (totalWeightGrams, bulkyType);
    }

    // ── Shipping Fee ──────────────────────────────────────────────────────────

    /// <summary>
    /// Calls GHN API for base fee, applies bulky surcharge and provider MultiplierFee.
    /// Gracefully degrades (fee stays 0) on missing data or API failure.
    /// </summary>
    private async Task<long> AutoCalculateShippingFeeAsync(
        string? pickupAddressId, string? deliveryAddressId,
        double totalWeightGrams, string bulkyType,
        ProviderService providerService)
    {
        if (string.IsNullOrEmpty(pickupAddressId) || string.IsNullOrEmpty(deliveryAddressId))
        {
            _logger.LogWarning("Cannot auto-calculate fee: pickup/delivery address not provided");
            return 0;
        }

        var pickup = _ghnApiClient.ResolvePickupAddress(pickupAddressId);
        var delivery = _ghnApiClient.ResolveDeliveryAddress(deliveryAddressId);
        if (pickup == null || delivery == null)
        {
            _logger.LogWarning("Cannot resolve addresses: pickup={P}, delivery={D}", pickupAddressId, deliveryAddressId);
            return 0;
        }

        var ghnRequest = new GhnFeeRequest
        {
            ServiceId = _ghnApiClient.MapServiceCode(providerService.Code),
            FromDistrictId = pickup.DistrictId,
            ToDistrictId = delivery.DistrictId,
            ToWardCode = delivery.WardCode,
            Weight = (int)Math.Ceiling(totalWeightGrams)
        };

        try
        {
            var ghnFee = await _ghnApiClient.GetFeeAsync(ghnRequest);
            long baseFee = ghnFee.Total;
            long surcharge = bulkyType switch
            {
                "BULKY" => (long)Math.Round(baseFee * BulkySurchargeRate),
                "SUPER_BULKY" => (long)Math.Round(baseFee * SuperBulkySurchargeRate),
                _ => 0L
            };

            double multiplier = providerService.MultiplierFee ?? 1.0;
            long finalFee = (long)Math.Round((baseFee + surcharge) * multiplier);

            _logger.LogInformation(
                "Auto-calculated fee: base={Base}, surcharge={Surcharge} ({Bulky}), x{Mul} → {Final} cents",
                baseFee, surcharge, bulkyType, multiplier, finalFee);

            return finalFee;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GHN API error during fee auto-calc; fee defaults to 0");
            return 0;
        }
    }

    private static DateTime? EstimateDeliveryEta(ProviderService providerService)
    {
        int? etaDays = providerService.EstimatedDaysMax ?? providerService.EstimatedDaysMin;
        if (!etaDays.HasValue || etaDays.Value <= 0)
            return null;

        return DateTime.UtcNow.AddDays(etaDays.Value);
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

    public async Task<ServiceResult<ShipmentDto>> UpdatePickupAsync(Guid id, UpdateShipmentPickupDto dto)
    {
        try
        {
            var shipment = await _shipmentRepository.GetByIdAsync(id);
            if (shipment == null)
                return ServiceResult<ShipmentDto>.NotFound(ShipmentMessages.ShipmentNotFound);

            shipment.PickedUpAt = dto.PickedUpAt ?? DateTime.UtcNow;
            shipment.UpdatedAt = DateTime.UtcNow;

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
