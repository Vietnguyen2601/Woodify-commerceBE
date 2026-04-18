using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace OrderService.Application.Services;

/// <summary>
/// Service để handle analytics operations
/// - Top selling categories
/// - Top selling products
/// - Sales metrics
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICategoryCacheRepository _categoryRepository;
    private readonly IProductMasterCacheRepository _productMasterRepository;
    private readonly IProductVersionCacheRepository _productVersionRepository;

    public AnalyticsService(
        IOrderRepository orderRepository,
        ICategoryCacheRepository categoryRepository,
        IProductMasterCacheRepository productMasterRepository,
        IProductVersionCacheRepository productVersionRepository)
    {
        _orderRepository = orderRepository;
        _categoryRepository = categoryRepository;
        _productMasterRepository = productMasterRepository;
        _productVersionRepository = productVersionRepository;
    }

    /// <summary>
    /// Lấy top N categories có sản phẩm bán chạy nhất
    /// Sắp xếp theo số lượng items bán từ các sản phẩm trong category
    /// </summary>
    public async Task<ServiceResult<IEnumerable<TopCategoryDto>>> GetTopSellCategoriesAsync(int topN = 10)
    {
        try
        {
            // Lấy tất cả orders COMPLETED để tính sales
            var orders = await _orderRepository.GetAllCompletedOrdersAsync();
            
            if (!orders.Any())
                return ServiceResult<IEnumerable<TopCategoryDto>>.Success(Enumerable.Empty<TopCategoryDto>(), "No sales data available");

            // Lấy tất cả active categories
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDict = categories.ToDictionary(c => c.CategoryId);

            // Lấy tất cả product masters
            var products = await _productMasterRepository.GetAllActivePublishedAsync();
            var productDict = products.ToDictionary(p => p.ProductId);

            // Warn nếu cache không có dữ liệu
            if (!products.Any())
            {
                Console.WriteLine("[OrderService] WARNING: No ProductMaster data in cache. Run initial sync to populate data.");
                return ServiceResult<IEnumerable<TopCategoryDto>>.Success(Enumerable.Empty<TopCategoryDto>(), 
                    "No product data in cache. Please run initial sync from ProductService.");
            }

            // Lấy tất cả product versions cho price info
            var versions = (await _productVersionRepository.GetAllAsync())
                .Where(v => !v.IsDeleted)
                .ToList();
            var versionDict = versions.ToDictionary(v => v.VersionId);

            // Group order items by category
            var categorySales = new Dictionary<Guid, (int totalItems, double totalRevenue)>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    // Tìm product từ version
                    if (versionDict.TryGetValue(item.VersionId, out var version))
                    {
                        if (productDict.TryGetValue(version.ProductId, out var product))
                        {
                            var categoryId = product.CategoryId;
                            
                            if (categorySales.ContainsKey(categoryId))
                            {
                                var current = categorySales[categoryId];
                                categorySales[categoryId] = (
                                    current.totalItems + item.Quantity,
                                    current.totalRevenue + (item.UnitPriceVnd * item.Quantity)
                                );
                            }
                            else
                            {
                                categorySales[categoryId] = (
                                    item.Quantity,
                                    item.UnitPriceVnd * item.Quantity
                                );
                            }
                        }
                    }
                }
            }

            if (!categorySales.Any())
            {
                return ServiceResult<IEnumerable<TopCategoryDto>>.Success(Enumerable.Empty<TopCategoryDto>(), 
                    "No sales data found for categories");
            }

            // Tạo DTOs và sort theo total items sold
            var result = categorySales
                .Select(kvp => new TopCategoryDto
                {
                    CategoryId = kvp.Key,
                    Name = categoryDict.TryGetValue(kvp.Key, out var cat) ? cat.Name : "Unknown",
                    Description = categoryDict.TryGetValue(kvp.Key, out var cat2) ? cat2.Description : null,
                    Level = categoryDict.TryGetValue(kvp.Key, out var cat3) ? cat3.Level : 0,
                    TotalItemsSold = kvp.Value.totalItems,
                    TotalSalesRevenue = kvp.Value.totalRevenue,
                    PublishedProductCount = products.Count(p => p.CategoryId == kvp.Key)
                })
                .OrderByDescending(x => x.TotalItemsSold)
                .ThenByDescending(x => x.TotalSalesRevenue)
                .Take(topN)
                .ToList();

            return ServiceResult<IEnumerable<TopCategoryDto>>.Success(result, "Top categories retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<TopCategoryDto>>.InternalServerError($"Error retrieving top categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy top N products bán chạy nhất trong một category
    /// </summary>
    public async Task<ServiceResult<IEnumerable<TopProductDto>>> GetTopProductsByCategoryAsync(Guid categoryId, int topN = 10)
    {
        try
        {
            // Verify category exists
            var category = await _categoryRepository.GetByCategoryIdAsync(categoryId);
            if (category == null)
                return ServiceResult<IEnumerable<TopProductDto>>.NotFound("Category not found");

            // Lấy tất cả orders COMPLETED
            var orders = await _orderRepository.GetAllCompletedOrdersAsync();
            
            if (!orders.Any())
                return ServiceResult<IEnumerable<TopProductDto>>.Success(Enumerable.Empty<TopProductDto>(), "No sales data available");

            // Lấy product masters trong category này
            var products = await _productMasterRepository.GetByCategoryIdAsync(categoryId);
            var productDict = products.ToDictionary(p => p.ProductId);

            // Warn nếu không có products trong category
            if (!products.Any())
            {
                Console.WriteLine($"[OrderService] WARNING: No ProductMaster data for category {categoryId}. Run initial sync to populate data.");
                return ServiceResult<IEnumerable<TopProductDto>>.Success(Enumerable.Empty<TopProductDto>(), 
                    "No product data in cache for this category");
            }

            // Lấy versions
            var versions = (await _productVersionRepository.GetAllAsync())
                .Where(v => !v.IsDeleted && productDict.ContainsKey(v.ProductId))
                .ToList();
            var versionDict = versions.ToDictionary(v => v.VersionId);

            // Group order items by product
            var productSales = new Dictionary<Guid, (int totalItems, double totalRevenue, double totalPrice)>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (versionDict.TryGetValue(item.VersionId, out var version))
                    {
                        var productId = version.ProductId;
                        
                        if (productSales.ContainsKey(productId))
                        {
                            var current = productSales[productId];
                            productSales[productId] = (
                                current.totalItems + item.Quantity,
                                current.totalRevenue + (item.UnitPriceVnd * item.Quantity),
                                current.totalPrice + item.UnitPriceVnd
                            );
                        }
                        else
                        {
                            productSales[productId] = (
                                item.Quantity,
                                item.UnitPriceVnd * item.Quantity,
                                item.UnitPriceVnd
                            );
                        }
                    }
                }
            }

            // Tạo DTOs và sort
            var result = productSales
                .Select(kvp => 
                {
                    var prod = productDict.TryGetValue(kvp.Key, out var p) ? p : null;
                    var avgPrice = kvp.Value.totalItems > 0 
                        ? kvp.Value.totalRevenue / kvp.Value.totalItems 
                        : 0;
                    
                    return new TopProductDto
                    {
                        ProductId = kvp.Key,
                        CategoryId = categoryId,
                        CategoryName = category.Name,
                        ShopId = prod?.ShopId ?? Guid.Empty,
                        Name = prod?.Name ?? "Unknown",
                        Description = prod?.Description,
                        Status = prod?.Status ?? "UNKNOWN",
                        TotalItemsSold = kvp.Value.totalItems,
                        TotalSalesRevenue = kvp.Value.totalRevenue,
                        AveragePrice = avgPrice,
                        ThumbnailUrl = versions.FirstOrDefault(v => v.ProductId == kvp.Key)?.ThumbnailUrl
                    };
                })
                .OrderByDescending(x => x.TotalItemsSold)
                .ThenByDescending(x => x.TotalSalesRevenue)
                .Take(topN)
                .ToList();

            return ServiceResult<IEnumerable<TopProductDto>>.Success(result, "Top products in category retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<TopProductDto>>.InternalServerError($"Error retrieving top products: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy top N products bán chạy nhất (tất cả categories)
    /// </summary>
    public async Task<ServiceResult<IEnumerable<TopProductDto>>> GetTopProductsOverallAsync(int topN = 20)
    {
        try
        {
            // Lấy tất cả orders COMPLETED
            var orders = await _orderRepository.GetAllCompletedOrdersAsync();
            
            if (!orders.Any())
                return ServiceResult<IEnumerable<TopProductDto>>.Success(Enumerable.Empty<TopProductDto>(), "No sales data available");

            // Lấy tất cả product masters
            var products = await _productMasterRepository.GetAllActivePublishedAsync();
            var productDict = products.ToDictionary(p => p.ProductId);

            // Warn nếu cache không có dữ liệu
            if (!products.Any())
            {
                Console.WriteLine("[OrderService] WARNING: No ProductMaster data in cache. Run initial sync to populate data.");
                return ServiceResult<IEnumerable<TopProductDto>>.Success(Enumerable.Empty<TopProductDto>(), 
                    "No product data in cache. Please run initial sync from ProductService.");
            }

            // Lấy categories
            var categories = await _categoryRepository.GetAllActiveAsync();
            var categoryDict = categories.ToDictionary(c => c.CategoryId);

            // Lấy versions
            var versions = (await _productVersionRepository.GetAllAsync())
                .Where(v => !v.IsDeleted)
                .ToList();
            var versionDict = versions.ToDictionary(v => v.VersionId);

            // Group order items by product
            var productSales = new Dictionary<Guid, (int totalItems, double totalRevenue, double totalPrice)>();

            foreach (var order in orders)
            {
                foreach (var item in order.OrderItems)
                {
                    if (versionDict.TryGetValue(item.VersionId, out var version))
                    {
                        var productId = version.ProductId;
                        
                        if (productSales.ContainsKey(productId))
                        {
                            var current = productSales[productId];
                            productSales[productId] = (
                                current.totalItems + item.Quantity,
                                current.totalRevenue + (item.UnitPriceVnd * item.Quantity),
                                current.totalPrice + item.UnitPriceVnd
                            );
                        }
                        else
                        {
                            productSales[productId] = (
                                item.Quantity,
                                item.UnitPriceVnd * item.Quantity,
                                item.UnitPriceVnd
                            );
                        }
                    }
                }
            }

            // Tạo DTOs và sort
            var result = productSales
                .Select(kvp => 
                {
                    var prod = productDict.TryGetValue(kvp.Key, out var p) ? p : null;
                    var cat = prod != null && categoryDict.TryGetValue(prod.CategoryId, out var c) ? c : null;
                    var avgPrice = kvp.Value.totalItems > 0 
                        ? kvp.Value.totalRevenue / kvp.Value.totalItems 
                        : 0;
                    
                    return new TopProductDto
                    {
                        ProductId = kvp.Key,
                        CategoryId = prod?.CategoryId ?? Guid.Empty,
                        CategoryName = cat?.Name ?? "Unknown",
                        ShopId = prod?.ShopId ?? Guid.Empty,
                        Name = prod?.Name ?? "Unknown",
                        Description = prod?.Description,
                        Status = prod?.Status ?? "UNKNOWN",
                        TotalItemsSold = kvp.Value.totalItems,
                        TotalSalesRevenue = kvp.Value.totalRevenue,
                        AveragePrice = avgPrice,
                        ThumbnailUrl = versions.FirstOrDefault(v => v.ProductId == kvp.Key)?.ThumbnailUrl
                    };
                })
                .OrderByDescending(x => x.TotalItemsSold)
                .ThenByDescending(x => x.TotalSalesRevenue)
                .Take(topN)
                .ToList();

            return ServiceResult<IEnumerable<TopProductDto>>.Success(result, "Top products retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<TopProductDto>>.InternalServerError($"Error retrieving top products: {ex.Message}");
        }
    }
}
