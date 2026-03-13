using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShipmentService.Application.Constants;
using ShipmentService.Application.DTOs;
using ShipmentService.Application.Interfaces;
using ShipmentService.Infrastructure.ExternalProviders;
using ShipmentService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ShipmentService.Application.Services;

public class ShippingFeePreviewAppService : IShippingFeePreviewService
{
    private const double BulkySurchargeRate = 0.20;
    private const double SuperBulkySurchargeRate = 0.50;

    private readonly IProviderServiceRepository _providerServiceRepository;
    private readonly IGhnApiClient _ghnApiClient;
    private readonly ILogger<ShippingFeePreviewAppService> _logger;

    public ShippingFeePreviewAppService(
        IProviderServiceRepository providerServiceRepository,
        IGhnApiClient ghnApiClient,
        ILogger<ShippingFeePreviewAppService> logger)
    {
        _providerServiceRepository = providerServiceRepository;
        _ghnApiClient = ghnApiClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ShippingFeePreviewResponse>> CalculateAsync(
        ShippingFeePreviewRequest request)
    {
        var validationError = ValidateInput(request);
        if (validationError != null)
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(validationError);

        var providerService = await _providerServiceRepository
            .GetByCodeAsync(request.ProviderServiceCode);

        if (providerService == null)
        {
            _logger.LogWarning(
                "ProviderService không tìm thấy hoặc không active: code='{Code}'",
                request.ProviderServiceCode);
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                ShipmentMessages.FeePreviewServiceNotAvailable);
        }

        _logger.LogInformation(
            "Tính phí ship: service={Code} ({Name}), weight={Weight}g, bulky={Bulky}",
            providerService.Code, providerService.Name,
            request.TotalWeightGrams, request.BulkyType);

        var pickup = _ghnApiClient.ResolvePickupAddress(request.PickupAddressId);
        var delivery = _ghnApiClient.ResolveDeliveryAddress(request.DeliveryAddressId);

        if (pickup == null || delivery == null)
        {
            return ServiceResult<ShippingFeePreviewResponse>.BadRequest(
                ShipmentMessages.FeePreviewInvalidAddress);
        }

        int ghnServiceId = ResolveGhnServiceId(request.ProviderServiceCode);

        var ghnRequest = new GhnFeeRequest
        {
            ServiceId = ghnServiceId,
            FromDistrictId = pickup.DistrictId,
            ToDistrictId = delivery.DistrictId,
            ToWardCode = delivery.WardCode,
            Weight = (int)Math.Ceiling(request.TotalWeightGrams),
            InsuranceValue = request.SubtotalCents
        };


        GhnFeeResponse ghnFee;
        try
        {
            ghnFee = await _ghnApiClient.GetFeeAsync(ghnRequest);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "GHN API lỗi khi tính phí: service={Code}, from={From}, to={To}",
                request.ProviderServiceCode, pickup.DistrictId, delivery.DistrictId);
            return ServiceResult<ShippingFeePreviewResponse>.InternalServerError(
                ShipmentMessages.FeePreviewProviderError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi không xác định khi gọi GHN API");
            return ServiceResult<ShippingFeePreviewResponse>.InternalServerError(
                ShipmentMessages.FeePreviewProviderError);
        }

        long baseFee = ghnFee.Total;
        long surcharge = request.BulkyType switch
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
            baseFee, surcharge, request.BulkyType, multiplier, rawFinal);

        // TODO: Wire vào VoucherService để check voucher_id freeship
        // TODO: Wire vào ShopService để lấy freeship_threshold theo shop_id
        var (isFreeShipping, freeShipReason) = CheckFreeShipping(request, rawFinal);
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
        if (string.IsNullOrWhiteSpace(request.ProviderServiceCode))
            return "provider_service_code là bắt buộc.";

        if (request.TotalWeightGrams <= 0)
            return "total_weight_grams phải lớn hơn 0.";

        var allowedBulky = new[] { "NORMAL", "BULKY", "SUPER_BULKY" };
        if (!allowedBulky.Contains(request.BulkyType?.ToUpperInvariant()))
            return "bulky_type phải là NORMAL, BULKY hoặc SUPER_BULKY.";

        if (string.IsNullOrWhiteSpace(request.PickupAddressId))
            return "pickup_address_id là bắt buộc.";

        if (string.IsNullOrWhiteSpace(request.DeliveryAddressId))
            return "delivery_address_id là bắt buộc.";

        return null;
    }

    private static (bool isFree, string reason) CheckFreeShipping(
        ShippingFeePreviewRequest request, long calculatedFee)
    {
        // TODO: Lấy threshold từ Shop.FreeshippingThreshold via ShopService
        const long freeshippingThresholdCents = 50_000_000L;
        if (request.SubtotalCents.HasValue &&
            request.SubtotalCents.Value >= freeshippingThresholdCents)
        {
            return (true, "Đơn hàng đủ điều kiện freeship (đơn ≥ 500.000đ)");
        }

        // TODO: Kiểm tra request.VoucherId freeship qua VoucherService

        return (false, string.Empty);
    }

    private static int ResolveGhnServiceId(string code) => code.ToUpperInvariant() switch
    {
        "ECO" => 5,
        "STD" => 5,
        "EXP" => 2,
        "SUP" => 1,
        _ => 5
    };
}
