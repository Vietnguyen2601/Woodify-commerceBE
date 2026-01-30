using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using ProductService.Domain.Parameters;
using Shared.Results;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ProductService.Application.Services;

public class ProductMasterService : IProductMasterService
{
    private readonly IProductMasterRepository _productMasterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private const string AllProductsCacheKey = "all_products";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public ProductMasterService(IProductMasterRepository productMasterRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _productMasterRepository = productMasterRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ServiceResult<ProductMasterDto>> GetByIdAsync(Guid id)
    {
        var product = await _productMasterRepository.GetByIdAsync(id);
        if (product == null)
            return ServiceResult<ProductMasterDto>.NotFound("Product not found");
        
        return ServiceResult<ProductMasterDto>.Success(product.ToDto());
    }

    public async Task<ServiceResult<ProductMasterDto>> GetByGlobalSkuAsync(string globalSku)
    {
        var product = await _productMasterRepository.GetByGlobalSkuAsync(globalSku);
        if (product == null)
            return ServiceResult<ProductMasterDto>.NotFound("Product not found");
        
        return ServiceResult<ProductMasterDto>.Success(product.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetAllAsync()
    {
        // Try to get from cache first
        var cachedData = await _cache.GetStringAsync(AllProductsCacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedProducts = JsonSerializer.Deserialize<IEnumerable<ProductMasterDto>>(cachedData);
            if (cachedProducts != null)
            {
                return ServiceResult<IEnumerable<ProductMasterDto>>.Success(cachedProducts);
            }
        }

        // Get from database if not in cache
        var products = await _productMasterRepository.GetAllAsync();
        var productDtos = products.Select(p => p.ToDto()).ToList();
        
        // Store in cache
        var serializedData = JsonSerializer.Serialize(productDtos);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheExpiration
        };
        await _cache.SetStringAsync(AllProductsCacheKey, serializedData, cacheOptions);
        
        return ServiceResult<IEnumerable<ProductMasterDto>>.Success(productDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetByShopIdAsync(Guid shopId)
    {
        var products = await _productMasterRepository.GetByShopIdAsync(shopId);
        var productDtos = products.Select(p => p.ToDto());
        
        return ServiceResult<IEnumerable<ProductMasterDto>>.Success(productDtos);
    }

    public async Task<ServiceResult<ProductMasterDto>> CreateAsync(CreateProductMasterDto dto)
    {
        try
        {
            // Validate Category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
                return ServiceResult<ProductMasterDto>.NotFound("Category not found");

            var product = dto.ToModel();
            await _productMasterRepository.CreateAsync(product);

            // Invalidate cache
            await _cache.RemoveAsync(AllProductsCacheKey);

            return ServiceResult<ProductMasterDto>.Created(product.ToDto(), "Product created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error creating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductMasterDto>> UpdateAsync(Guid id, UpdateProductMasterDto dto)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductMasterDto>.NotFound("Product not found");

            // Validate Category if being updated
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                    return ServiceResult<ProductMasterDto>.NotFound("Category not found");
            }

            dto.MapToUpdate(product);
            await _productMasterRepository.UpdateAsync(product);
            
            // Invalidate cache
            await _cache.RemoveAsync(AllProductsCacheKey);
            
            var updatedProduct = await _productMasterRepository.GetByIdAsync(id);
            return ServiceResult<ProductMasterDto>.Success(updatedProduct!.ToDto(), "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error updating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult.NotFound("Product not found");
            
            await _productMasterRepository.RemoveAsync(product);
            
            // Invalidate cache
            await _cache.RemoveAsync(AllProductsCacheKey);
            
            return ServiceResult.Success("Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductMasterDto>> ArchiveProductAsync(Guid id)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductMasterDto>.NotFound("Product not found");

            if (product.Status == Domain.Entities.ProductStatus.ARCHIVED)
                return ServiceResult<ProductMasterDto>.BadRequest("Product is already archived");

            product.Status = Domain.Entities.ProductStatus.ARCHIVED;
            product.UpdatedAt = DateTime.UtcNow;
            await _productMasterRepository.UpdateAsync(product);

            // Invalidate cache
            await _cache.RemoveAsync(AllProductsCacheKey);

            return ServiceResult<ProductMasterDto>.Success(product.ToDto(), "Product archived successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error archiving product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetArchivedProductsAsync()
    {
        try
        {
            var archivedProducts = await _productMasterRepository.GetByStatusAsync(Domain.Entities.ProductStatus.ARCHIVED);
            var productDtos = archivedProducts.Select(p => p.ToDto());
            
            return ServiceResult<IEnumerable<ProductMasterDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductMasterDto>>.InternalServerError($"Error retrieving archived products: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductMasterDto>> PublishProductAsync(Guid id)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductMasterDto>.NotFound("Product not found");

            // Chỉ cho phép chuyển từ DRAFT hoặc ARCHIVED sang PUBLISHED
            if (product.Status == Domain.Entities.ProductStatus.DELETED)
                return ServiceResult<ProductMasterDto>.BadRequest("Cannot publish a deleted product");

            if (product.Status == Domain.Entities.ProductStatus.PUBLISHED)
                return ServiceResult<ProductMasterDto>.BadRequest("Product is already published");

            product.Status = Domain.Entities.ProductStatus.PUBLISHED;
            product.UpdatedAt = DateTime.UtcNow;
            await _productMasterRepository.UpdateAsync(product);

            // Invalidate cache
            await _cache.RemoveAsync(AllProductsCacheKey);

            return ServiceResult<ProductMasterDto>.Success(product.ToDto(), "Product published successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error publishing product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetPublishedProductsAsync()
    {
        try
        {
            var publishedProducts = await _productMasterRepository.GetByStatusAsync(Domain.Entities.ProductStatus.PUBLISHED);
            var productDtos = publishedProducts.Select(p => p.ToDto());
            
            return ServiceResult<IEnumerable<ProductMasterDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductMasterDto>>.InternalServerError($"Error retrieving published products: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductSearchResultDto>> SearchProductsAsync(ProductSearchDto searchDto)
    {
        try
        {
            // Convert DTO to Parameters
            var searchParams = new ProductSearchParameters
            {
                Keyword = searchDto.Keyword,
                Status = searchDto.Status?.ToString() ?? "PUBLISHED",
                CategoryName = searchDto.CategoryName,
                MinRating = (double?)searchDto.MinRating,
                MaxRating = (double?)searchDto.MaxRating,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                SortBy = searchDto.SortBy,
                SortDirection = searchDto.SortDirection
            };

            var (products, totalCount) = await _productMasterRepository.SearchAsync(searchParams);
            
            // Get current version info for each product
            var productDtos = new List<ProductMasterDto>();
            foreach (var product in products)
            {
                var dto = product.ToDto();
                
                // Get current version details if available
                if (product.CurrentVersionId.HasValue)
                {
                    var version = await _unitOfWork.ProductVersions.GetByIdAsync(product.CurrentVersionId.Value);
                    if (version != null)
                    {
                        dto.CurrentVersionTitle = version.Title;
                        dto.CurrentVersionDescription = version.Description;
                    }
                }
                
                productDtos.Add(dto);
            }
            
            var result = new ProductSearchResultDto
            {
                Products = productDtos,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
            
            return ServiceResult<ProductSearchResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductSearchResultDto>.InternalServerError($"Error searching products: {ex.Message}");
        }
    }
}
