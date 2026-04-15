using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Shared.Results;
using System.Text.Json;

namespace OrderService.Application.Services;

/// <summary>
/// Service để thực hiện initial sync dữ liệu từ ProductService
/// Populate CategoryCache và ProductMasterCache khi OrderService startup
/// </summary>
public class InitialSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ICategoryCacheRepository _categoryCacheRepository;
    private readonly IProductMasterCacheRepository _productMasterCacheRepository;
    private readonly string _productServiceUrl;

    public InitialSyncService(
        HttpClient httpClient,
        ICategoryCacheRepository categoryCacheRepository,
        IProductMasterCacheRepository productMasterCacheRepository,
        string? productServiceUrl = null)
    {
        _httpClient = httpClient;
        _categoryCacheRepository = categoryCacheRepository;
        _productMasterCacheRepository = productMasterCacheRepository;
        // Default ProductService URL - override bằng env var hoặc config
        _productServiceUrl = productServiceUrl ?? (
            Environment.GetEnvironmentVariable("ProductService_URL") ?? "http://product-service:5000"
        );
    }

    /// <summary>
    /// Thực hiện initial sync: lấy tất cả categories và products từ ProductService
    /// Populate CategoryCache và ProductMasterCache
    /// </summary>
    public async Task SyncAllDataAsync()
    {
        try
        {
            Console.WriteLine("[OrderService] Starting initial data sync from ProductService...");
            
            await SyncCategoriesAsync();
            await SyncProductsAsync();
            
            Console.WriteLine("[OrderService] Initial data sync completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] ERROR during initial sync: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Sync tất cả categories từ ProductService
    /// </summary>
    private async Task SyncCategoriesAsync()
    {
        try
        {
            Console.WriteLine("[OrderService] Syncing categories from ProductService...");
            
            var response = await _httpClient.GetAsync($"{_productServiceUrl}/api/sync/categories");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[OrderService] Failed to fetch categories: {response.StatusCode}");
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            
            // Parse ServiceResult<IEnumerable<CategoryDto>>
            if (root.TryGetProperty("data", out var dataElement))
            {
                var categories = JsonSerializer.Deserialize<List<CategoryCacheDto>>(
                    dataElement.GetRawText(), 
                    options
                ) ?? new List<CategoryCacheDto>();

                Console.WriteLine($"[OrderService] Found {categories.Count} categories to sync");

                foreach (var catDto in categories)
                {
                    var cache = new CategoryCache
                    {
                        CategoryId = catDto.CategoryId,
                        Name = catDto.Name,
                        Description = catDto.Description,
                        ParentCategoryId = catDto.ParentCategoryId,
                        Level = catDto.Level,
                        IsActive = true,
                        LastUpdated = DateTime.UtcNow
                    };

                    await _categoryCacheRepository.UpsertAsync(cache);
                }

                Console.WriteLine($"[OrderService] Categories synced successfully: {categories.Count}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error syncing categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Sync tất cả products từ ProductService
    /// </summary>
    private async Task SyncProductsAsync()
    {
        try
        {
            Console.WriteLine("[OrderService] Syncing products from ProductService...");
            
            var response = await _httpClient.GetAsync($"{_productServiceUrl}/api/sync/products");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[OrderService] Failed to fetch products: {response.StatusCode}");
                return;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            
            // Parse ServiceResult<IEnumerable<ProductMasterDto>>
            if (root.TryGetProperty("data", out var dataElement))
            {
                var products = JsonSerializer.Deserialize<List<ProductMasterCacheDto>>(
                    dataElement.GetRawText(), 
                    options
                ) ?? new List<ProductMasterCacheDto>();

                Console.WriteLine($"[OrderService] Found {products.Count} products to sync");

                foreach (var prodDto in products)
                {
                    var cache = new ProductMasterCache
                    {
                        ProductId = prodDto.ProductId,
                        ShopId = prodDto.ShopId,
                        CategoryId = prodDto.CategoryId,
                        Name = prodDto.Name,
                        Description = prodDto.Description,
                        Status = prodDto.Status,
                        ModerationStatus = prodDto.ModerationStatus,
                        HasVersions = prodDto.HasVersions,
                        LastUpdated = DateTime.UtcNow
                    };

                    await _productMasterCacheRepository.UpsertAsync(cache);
                }

                Console.WriteLine($"[OrderService] Products synced successfully: {products.Count}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OrderService] Error syncing products: {ex.Message}");
        }
    }

    /// <summary>
    /// DTOs để deserialize responses từ ProductService
    /// </summary>
    private class CategoryCacheDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int Level { get; set; }
    }

    private class ProductMasterCacheDto
    {
        public Guid ProductId { get; set; }
        public Guid ShopId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ModerationStatus { get; set; }
        public bool HasVersions { get; set; }
    }
}
