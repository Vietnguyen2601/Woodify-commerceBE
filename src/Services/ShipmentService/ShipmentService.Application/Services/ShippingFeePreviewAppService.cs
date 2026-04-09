using Microsoft.Extensions.Logging;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Repositories.IRepositories;
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

        var orderInfo = await _orderInfoCache.GetOrderInfoAsync(request.OrderId);
        var shopInfo = await _shopInfoCache.GetShopInfoAsync(request.ShopId);

        if (orderInfo is not null && orderInfo.ShopId != request.ShopId)
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                "order_id does not belong to the given shop_id.");

        double totalWeightGrams = orderInfo is { TotalWeightGrams: > 0 }
            ? orderInfo.TotalWeightGrams
            : 1000.0;

        string bulkyType = CalculateBulkyType(totalWeightGrams);
        long subtotalCents = (long)(orderInfo?.TotalAmountCents ?? 10_000_000L);

        string providerServiceCode = !string.IsNullOrEmpty(request.ProviderServiceCode)
            ? request.ProviderServiceCode
            : orderInfo?.ProviderServiceCode
              ?? shopInfo?.DefaultProviderServiceCode
              ?? "STD";

        var providerService = await _providerServiceRepository
            .GetByCodeAsync(providerServiceCode);

        if (providerService == null)
        {
            var safeCode = providerServiceCode?.Replace("\r", "").Replace("\n", "");
            _logger.LogWarning("ProviderService not found: code='{Code}'", safeCode);
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                ShipmentMessages.FeePreviewServiceNotAvailable);
        }

        _logger.LogInformation(
            "Fee preview: service={Code} ({Name}), weight={Weight}g, bulky={Bulky}",
            providerService.Code, providerService.Name, totalWeightGrams, bulkyType);

        int serviceId = _feeCalculator.MapServiceCode(providerServiceCode);
        int weightGrams = (int)Math.Ceiling(totalWeightGrams);

        ShippingFeeResult feeResult;
        try
        {
            feeResult = await _feeCalculator.CalculateAsync(serviceId, weightGrams);
        }
        catch (Exception ex)
        {
            var safeCode = (providerServiceCode ?? string.Empty).Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
            _logger.LogError(ex, "Fee calculator error: service={Code}", safeCode);
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

        var (isFreeShipping, freeShipReason) = CheckFreeShipping(subtotalCents);
        long finalFee = isFreeShipping ? 0L : rawFinal;

        string message = isFreeShipping
            ? $"Miễn phí vận chuyển — {freeShipReason}"
            : ShipmentMessages.FeePreviewSuccess;

        return ServiceResult<ShippingFeePreviewResponse>.Success(
            new ShippingFeePreviewResponse
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
            },
            message);
    }

    private static string? ValidateInput(ShippingFeePreviewRequest request)
    {
        if (request.ShopId == Guid.Empty)
            return "shop_id là bắt buộc.";
        if (request.OrderId == Guid.Empty)
            return "order_id là bắt buộc.";
        return null;
    }

    private static (bool isFree, string reason) CheckFreeShipping(long subtotalCents)
    {
        const long threshold = 50_000_000L;
        if (subtotalCents >= threshold)
            return (true, "Đơn hàng đủ điều kiện freeship (đơn ≥ 500.000đ)");
        return (false, string.Empty);
    }

    private static string CalculateBulkyType(double weightGrams)
    {
        if (weightGrams >= 5000.0) return "SUPER_BULKY";
        if (weightGrams >= 2000.0) return "BULKY";
        return "NORMAL";
    }
}
