using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Infrastructure.Services;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using ShipmentService.Infrastructure.Cache;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShippingFeePreviewAppService : IShippingFeePreviewService
{
    private const double BulkySurchargeRate = 0.20;
    private const double SuperBulkySurchargeRate = 0.50;

    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IShippingFeeCalculator _feeCalculator;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly ILogger<ShippingFeePreviewAppService> _logger;

    public ShippingFeePreviewAppService(
        IProviderServiceRepository providerServiceRepository,
        IShippingFeeCalculator feeCalculator,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache,
        ILogger<ShippingFeePreviewAppService> logger)
    {
        _providerServiceRepository = providerServiceRepository;
        _feeCalculator = feeCalculator;
        _orderInfoCache = orderInfoCache;
        _shopInfoCache = shopInfoCache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ShippingFeePreviewResponse>> CalculateAsync(
        ShippingFeePreviewRequest request)
    {
        var validationError = ValidateInput(request);
        if (validationError != null)
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(validationError);

        // Get shop info from cache (optional - use fallback if not available)
        var cachedShopInfo = await _shopInfoCache.GetShopInfoAsync(request.ShopId);

        // Get order info from cache (optional - use fallback if not available)
        var cachedOrderInfo = await _orderInfoCache.GetOrderInfoAsync(request.OrderId);

        // Auto-calculate fields from order (or use defaults)
        double totalWeightGrams = CalculateWeight(null); // Mock: 1000g default
        string bulkyType = CalculateBulkyType(totalWeightGrams);
        long subtotalCents = (long)(cachedOrderInfo?.TotalAmountCents ?? 10000000); // Default 100k VND if no order

        // Use provider from request, fallback to shop's default provider
        string providerServiceCode = !string.IsNullOrEmpty(request.ProviderServiceCode)
            ? request.ProviderServiceCode
            : cachedShopInfo?.DefaultProviderServiceCode ?? "STD";

        var providerService = await _providerServiceRepository
            .GetByCodeAsync(providerServiceCode);

        if (providerService == null)
        {
            _logger.LogWarning(
                "ProviderService không tìm thấy hoặc không active: code='{Code}'",
                providerServiceCode);
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                ShipmentMessages.FeePreviewServiceNotAvailable);
        }

        _logger.LogInformation(
            "Tính phí ship: service={Code} ({Name}), weight={Weight}g, bulky={Bulky}",
            providerService.Code, providerService.Name,
            totalWeightGrams, bulkyType);

        // Mock calculation - no address parsing, no district/ward logic
        int serviceId = _feeCalculator.MapServiceCode(providerServiceCode);
        int weightGrams = (int)Math.Ceiling(totalWeightGrams);

        ShippingFeeResult feeResult;
        try
        {
            feeResult = await _feeCalculator.CalculateAsync(serviceId, weightGrams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính phí ship: service={Code}", request.ProviderServiceCode);
            return ServiceResult<ShippingFeePreviewResponse>.InternalServerError(
                ShipmentMessages.FeePreviewProviderError);
        }

        long baseFee = feeResult.Total;
        long surcharge = bulkyType switch
        {
            "BULKY" => (long)Math.Round(baseFee * BulkySurchargeRate),
            "SUPER_BULKY" => (long)Math.Round(baseFee * SuperBulkySurchargeRate),
            _ => 0L
        };

        double multiplier = providerService.MultiplierFee ?? 1.0;
        long rawFinal = (long)Math.Round((baseFee + surcharge) * multiplier);

        _logger.LogInformation(
            "Fee breakdown: base={Base}, surcharge={Surcharge} ({Bulky}), " +
            "multiplier={Multiplier} → final={Final} cents",
            baseFee, surcharge, bulkyType, multiplier, rawFinal);

        // TODO: Wire vào VoucherService để check voucher_id freeship
        // TODO: Wire vào ShopService để lấy freeship_threshold theo shop_id
        var (isFreeShipping, freeShipReason) = CheckFreeShipping(subtotalCents, request.VoucherId, rawFinal);
        long finalFee = isFreeShipping ? 0L : rawFinal;

        string message = isFreeShipping
            ? $"Miễn phí vận chuyển — {freeShipReason}"
            : ShipmentMessages.FeePreviewSuccess;

        var responseData = new ShippingFeePreviewResponse
        {
            ProviderServiceCode = providerService.Code,
            ServiceName = providerService.Name,
            BaseFeeCents = baseFee,
            SurchargeCents = surcharge,
            MultiplierFee = multiplier,
            FinalShippingFeeCents = finalFee,
            IsFreeShipping = isFreeShipping,
            EstimatedDaysMin = providerService.EstimatedDaysMin,
            EstimatedDaysMax = providerService.EstimatedDaysMax,
            Message = message
        };

        return ServiceResult<ShippingFeePreviewResponse>.Success(responseData, message);
    }


    private static string? ValidateInput(ShippingFeePreviewRequest request)
    {
        if (request.ShopId == Guid.Empty)
            return "shop_id là bắt buộc.";

        if (request.OrderId == Guid.Empty)
            return "order_id là bắt buộc.";

        return null;
    }

    private static (bool isFree, string reason) CheckFreeShipping(
        long subtotalCents, Guid? voucherId, long calculatedFee)
    {
        // TODO: Lấy threshold từ Shop.FreeshippingThreshold via ShopService
        const long freeshippingThresholdCents = 50_000_000L;
        if (subtotalCents >= freeshippingThresholdCents)
        {
            return (true, "Đơn hàng đủ điều kiện freeship (đơn ≥ 500.000đ)");
        }

        // TODO: Kiểm tra voucherId freeship qua VoucherService

        return (false, string.Empty);
    }

    private static double CalculateWeight(List<OrderItemInfo>? items)
    {
        if (items == null || items.Count == 0)
            return 1000.0; // Default 1kg if no items

        return items.Sum(i => (i.Quantity * 500.0)); // Assume 500g per item as placeholder
    }

    private static string CalculateBulkyType(double weightGrams)
    {
        if (weightGrams >= 5000.0) // >= 5kg
            return "SUPER_BULKY";
        if (weightGrams >= 2000.0) // >= 2kg
            return "BULKY";
        return "NORMAL";
    }
}

