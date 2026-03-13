using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ShipmentService.Infrastructure.ExternalProviders;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductServiceClient(HttpClient http, ILogger<ProductServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ProductVersionInfo?> GetProductVersionAsync(Guid versionId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/ProductVersions/GetVersionById/{versionId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ProductService returned {Status} for version {VersionId}", response.StatusCode, versionId);
                return null;
            }

            var raw = await response.Content.ReadAsStringAsync();
            using var wrapper = JsonDocument.Parse(raw);

            if (!wrapper.RootElement.TryGetProperty("data", out var data))
            {
                _logger.LogWarning("ProductService response for {VersionId} missing 'data' field", versionId);
                return null;
            }

            return JsonSerializer.Deserialize<ProductVersionInfo>(data.GetRawText(), JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling ProductService for version {VersionId}", versionId);
            return null;
        }
    }
}
