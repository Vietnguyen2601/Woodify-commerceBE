using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace ShipmentService.Infrastructure.ExternalProviders;

public class GhnApiClient : IGhnApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GhnApiClient> _logger;
    private readonly string _shopToken;
    private readonly int _ghnShopId;
    private readonly Dictionary<string, int> _serviceMapping;

    private static readonly Dictionary<string, int> DefaultServiceMapping = new()
    {
        ["ECO"] = 5,
        ["STD"] = 5,
        ["EXP"] = 2,
        ["SUP"] = 1,
    };

    public GhnApiClient(HttpClient http, IConfiguration config, ILogger<GhnApiClient> logger)
    {
        _http = http;
        _logger = logger;

        var section = config.GetSection("GhnSettings");
        _shopToken = section["ShopToken"] ?? string.Empty;
        _ghnShopId = int.TryParse(section["GhnShopId"], out var shopId) ? shopId : 0;

        var mappingSection = section.GetSection("ServiceMapping");
        _serviceMapping = mappingSection.Exists()
            ? mappingSection.GetChildren().ToDictionary(c => c.Key, c => int.Parse(c.Value ?? "5"))
            : new Dictionary<string, int>(DefaultServiceMapping);
    }

    public async Task<GhnFeeResponse> GetFeeAsync(GhnFeeRequest request)
    {
        var body = new
        {
            service_id = request.ServiceId,
            from_district_id = request.FromDistrictId,
            to_district_id = request.ToDistrictId,
            to_ward_code = request.ToWardCode,
            weight = request.Weight,
            insurance_value = request.InsuranceValue ?? 0
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            "/shiip/public-api/v2/shipping-order/fee");

        httpRequest.Headers.Add("Token", _shopToken);
        httpRequest.Headers.Add("ShopId", _ghnShopId.ToString());
        httpRequest.Content = JsonContent.Create(body);

        _logger.LogInformation(
            "GHN fee request: service_id={ServiceId}, from={From}, to={To}/{Ward}, weight={Weight}g",
            request.ServiceId, request.FromDistrictId, request.ToDistrictId,
            request.ToWardCode, request.Weight);

        var response = await _http.SendAsync(httpRequest);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GHN API responded {Status}: {Body}", response.StatusCode, raw);
            throw new HttpRequestException(
                $"GHN API trả lỗi {(int)response.StatusCode}: {raw}");
        }

        var wrapper = JsonDocument.Parse(raw);
        if (!wrapper.RootElement.TryGetProperty("data", out var data))
            throw new InvalidOperationException($"GHN response thiếu 'data': {raw}");

        var fee = new GhnFeeResponse
        {
            Total = data.TryGetProperty("total", out var t) ? t.GetInt64() : 0,
            ServiceFee = data.TryGetProperty("service_fee", out var s) ? s.GetInt64() : 0,
            InsuranceFee = data.TryGetProperty("insurance_fee", out var i) ? i.GetInt64() : 0,
            CouponValue = data.TryGetProperty("coupon_value", out var c) ? c.GetInt64() : 0,
        };

        _logger.LogInformation("GHN fee result: total={Total}đ", fee.Total);
        return fee;
    }

    public GhnAddressInfo? ResolvePickupAddress(string pickupAddressId)
    {
        if (int.TryParse(pickupAddressId.Trim(), out int districtId))
            return new GhnAddressInfo { DistrictId = districtId, WardCode = string.Empty };

        _logger.LogWarning("Không thể phân giải pickup_address_id '{Id}' sang GHN district.", pickupAddressId);
        return null;
    }

    public GhnAddressInfo? ResolveDeliveryAddress(string deliveryAddressId)
    {
        // format: "{district_id}_{ward_code}"
        var parts = deliveryAddressId.Trim().Split('_', 2);
        if (parts.Length == 2 && int.TryParse(parts[0], out int districtId))
            return new GhnAddressInfo { DistrictId = districtId, WardCode = parts[1] };

        _logger.LogWarning("Không thể phân giải delivery_address_id '{Id}' sang GHN district/ward.", deliveryAddressId);
        return null;
    }

    public int MapServiceCode(string providerServiceCode)
    {
        if (_serviceMapping.TryGetValue(providerServiceCode.ToUpperInvariant(), out int id))
            return id;

        _logger.LogWarning("Không tìm thấy mapping GHN service_id cho code '{Code}', dùng default 5.", providerServiceCode);
        return 5; // Standard fallback
    }
}
