using Microsoft.Extensions.Logging;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Infrastructure.Cache;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Constants;
using Shared.Results;
using Shared.Shipping;

namespace ShipmentService.Application.Services;

public class ShippingFeePreviewAppService : IShippingFeePreviewService
{
    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IOrderInfoCacheRepository _orderInfoCache;
    private readonly IShopInfoCacheRepository _shopInfoCache;
    private readonly ILogger<ShippingFeePreviewAppService> _logger;

    public ShippingFeePreviewAppService(
        IProviderServiceRepository providerServiceRepository,
        IOrderInfoCacheRepository orderInfoCache,
        IShopInfoCacheRepository shopInfoCache,
        ILogger<ShippingFeePreviewAppService> logger)
    {
        _providerServiceRepository = providerServiceRepository;
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

        double subtotalVnd = orderInfo?.SubtotalVnd > 0
            ? orderInfo.SubtotalVnd
            : 0;

        string providerServiceCode = !string.IsNullOrEmpty(request.ProviderServiceCode)
            ? request.ProviderServiceCode
            : orderInfo?.ProviderServiceCode
              ?? shopInfo?.DefaultProviderServiceCode
              ?? ShippingServiceConstants.DEFAULT_PROVIDER_SERVICE_CODE;

        var canon = ShippingServiceConstants.CanonicalizeProviderServiceCode(providerServiceCode);
        if (!ShippingServiceConstants.IsValidServiceCode(canon))
        {
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                $"Invalid provider_service_code: {providerServiceCode}");
        }

        int weightGrams = (int)Math.Ceiling(totalWeightGrams);
        long feeBeforeFree = ShippingPricing.ComputeShippingFeeVnd(canon, weightGrams);
        long finalFee = ShippingPricing.FinalShippingFeeVnd(canon, weightGrams, subtotalVnd);
        bool isFreeShipping = subtotalVnd >= ShippingPricing.FreeShippingSubtotalThresholdVnd;

        var providerService = await _providerServiceRepository.GetByCodeAsync(canon);

        _logger.LogInformation(
            "Fee preview (unified): service={Code}, weight={Weight}g, subtotal={Subtotal}, fee={Fee}",
            canon, totalWeightGrams, subtotalVnd, finalFee);

        string message = isFreeShipping
            ? $"Free shipping — subtotal ≥ {ShippingPricing.FreeShippingSubtotalThresholdVnd:N0} VND"
            : ShipmentMessages.FeePreviewSuccess;

        return ServiceResult<ShippingFeePreviewResponse>.Success(
            new ShippingFeePreviewResponse
            {
                ProviderServiceCode = canon,
                ServiceName = providerService?.Name,
                BaseFeeVnd = feeBeforeFree,
                SurchargeVnd = 0,
                MultiplierFee = 1.0,
                FinalShippingFeeVnd = finalFee,
                IsFreeShipping = isFreeShipping,
                EstimatedDaysMin = providerService?.EstimatedDaysMin,
                EstimatedDaysMax = providerService?.EstimatedDaysMax,
                Message = message
            },
            message);
    }

    private static string? ValidateInput(ShippingFeePreviewRequest request)
    {
        if (request.ShopId == Guid.Empty)
            return "shop_id is required.";
        if (request.OrderId == Guid.Empty)
            return "order_id is required.";
        return null;
    }
}
