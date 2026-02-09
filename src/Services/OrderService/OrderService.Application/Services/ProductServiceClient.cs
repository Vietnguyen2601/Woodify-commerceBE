using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace OrderService.Application.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _productServiceUrl;

    public ProductServiceClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _productServiceUrl = Environment.GetEnvironmentVariable("ProductService_Url") 
                           ?? configuration["Services:ProductService:Url"] 
                           ?? "http://localhost:5001";
    }

    public async Task<ProductVersionDto?> GetProductVersionAsync(Guid versionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_productServiceUrl}/api/ProductVersions/GetProductVersionById/{versionId}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var serviceResponse = JsonSerializer.Deserialize<ProductServiceResponse<ProductVersionDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return serviceResponse?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsProductPublishedAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_productServiceUrl}/api/ProductMasters/GetProductMasterById/{productId}");
            
            if (!response.IsSuccessStatusCode)
                return false;

            var content = await response.Content.ReadAsStringAsync();
            var serviceResponse = JsonSerializer.Deserialize<ProductServiceResponse<dynamic>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (serviceResponse?.Data == null)
                return false;

            var statusProperty = ((JsonElement)serviceResponse.Data).GetProperty("status");
            return statusProperty.GetString()?.Equals("PUBLISHED", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }
}
