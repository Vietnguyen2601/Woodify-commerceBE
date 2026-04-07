using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using Shared.Results;
using Shared.Events;

namespace ProductService.Application.Services;

public class ProductVersionService : IProductVersionService
{
    private readonly IProductVersionRepository _productVersionRepository;
    private readonly IProductMasterRepository _productMasterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductEventPublisher _eventPublisher;
    private readonly IImageUrlRepository _imageUrlRepository;

    public ProductVersionService(
        IProductVersionRepository productVersionRepository,
        IProductMasterRepository productMasterRepository,
        IUnitOfWork unitOfWork,
        ProductEventPublisher eventPublisher,
        IImageUrlRepository imageUrlRepository)
    {
        _productVersionRepository = productVersionRepository;
        _productMasterRepository = productMasterRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _imageUrlRepository = imageUrlRepository;
    }

    public async Task<ServiceResult<ProductVersionDto>> GetByIdAsync(Guid id)
    {
        var version = await _productVersionRepository.GetByIdAsync(id);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("Product version not found");

        var dto = version.ToDto();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { id });
        dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(id);
        return ServiceResult<ProductVersionDto>.Success(dto);
    }

    public async Task<ServiceResult<ProductVersionDto>> GetBySellerSkuAsync(string sellerSku)
    {
        var version = await _productVersionRepository.GetBySellerSkuAsync(sellerSku);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("Product version not found");

        var dto = version.ToDto();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
        dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(version.VersionId);
        return ServiceResult<ProductVersionDto>.Success(dto);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetAllAsync()
    {
        var versions = await _productVersionRepository.GetAllAsync();
        var versionList = versions.ToList();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", versionList.Select(v => v.VersionId));
        var versionDtos = versionList.Select(v => { var dto = v.ToDto(); dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(v.VersionId); return dto; });

        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetByProductIdAsync(Guid productId)
    {
        var versions = await _productVersionRepository.GetByProductIdAsync(productId);
        var versionList = versions.ToList();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", versionList.Select(v => v.VersionId));
        var versionDtos = versionList.Select(v => { var dto = v.ToDto(); dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(v.VersionId); return dto; });

        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<ProductVersionDto>> GetLatestVersionByProductIdAsync(Guid productId)
    {
        var version = await _productVersionRepository.GetLatestVersionByProductIdAsync(productId);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("No version found for this product");

        var dto = version.ToDto();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
        dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(version.VersionId);
        return ServiceResult<ProductVersionDto>.Success(dto);
    }

    public async Task<ServiceResult<ProductVersionDto>> GetDefaultVersionByProductIdAsync(Guid productId)
    {
        var version = await _productVersionRepository.GetDefaultVersionByProductIdAsync(productId);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("No default version found for this product");

        var dto = version.ToDto();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
        dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(version.VersionId);
        return ServiceResult<ProductVersionDto>.Success(dto);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetInactiveVersionsAsync()
    {
        var versions = await _productVersionRepository.GetInactiveVersionsAsync();
        var versionList = versions.ToList();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", versionList.Select(v => v.VersionId));
        var versionDtos = versionList.Select(v => { var dto = v.ToDto(); dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(v.VersionId); return dto; });

        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetActiveVersionsAsync()
    {
        var versions = await _productVersionRepository.GetActiveVersionsAsync();
        var versionList = versions.ToList();
        var thumbnailMap = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", versionList.Select(v => v.VersionId));
        var versionDtos = versionList.Select(v => { var dto = v.ToDto(); dto.ThumbnailUrl = thumbnailMap.GetValueOrDefault(v.VersionId); return dto; });

        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<ProductVersionDto>> CreateAsync(CreateProductVersionDto dto)
    {
        try
        {
            // Validate that ProductId exists
            var product = await _productMasterRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
                return ServiceResult<ProductVersionDto>.NotFound($"Product with ID {dto.ProductId} not found");

            // Check if SellerSku already exists
            var existingSku = await _productVersionRepository.GetBySellerSkuAsync(dto.SellerSku);
            if (existingSku != null)
                return ServiceResult<ProductVersionDto>.BadRequest($"SellerSku '{dto.SellerSku}' already exists");

            // Create version
            var version = dto.ToModel();
            await _productVersionRepository.CreateAsync(version);

            // Check if this is the first version (GlobalSku is empty or null)
            bool isFirstVersion = string.IsNullOrEmpty(product.GlobalSku);

            // Create a new tracked instance for update
            var productToUpdate = new Domain.Entities.ProductMaster
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                CategoryId = product.CategoryId,
                Name = product.Name,
                GlobalSku = product.GlobalSku, // Keep existing GlobalSku by default
                Description = product.Description,
                Status = product.Status,
                ModerationStatus = product.ModerationStatus,
                ModeratedAt = product.ModeratedAt,
                CreatedAt = product.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = product.PublishedAt
            };

            // Only generate and set GlobalSku for the first version
            if (isFirstVersion)
            {
                var globalSku = await GenerateGlobalSkuAsync(product.ProductId, product.CategoryId);
                productToUpdate.GlobalSku = globalSku;
            }

            await _productMasterRepository.UpdateAsync(productToUpdate);

            // Publish event to OrderService
            var thumbnailMapCreated = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
            var thumbnailUrlCreated = thumbnailMapCreated.GetValueOrDefault(version.VersionId);
            _eventPublisher.PublishProductVersionUpdated(new ProductVersionUpdatedEvent
            {
                VersionId = version.VersionId,
                ProductId = version.ProductId,
                ShopId = product.ShopId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                ProductStatus = product.Status.ToString(),
                SellerSku = version.SellerSku,
                VersionNumber = version.VersionNumber,
                VersionName = version.VersionName,
                Price = version.Price,
                Currency = "VND",
                StockQuantity = version.StockQuantity,
                WoodType = version.WoodType,
                WeightGrams = version.WeightGrams,
                LengthCm = version.LengthCm,
                WidthCm = version.WidthCm,
                HeightCm = version.HeightCm,
                IsActive = version.IsActive,
                ThumbnailUrl = thumbnailUrlCreated,
                UpdatedAt = DateTime.UtcNow,
                EventType = "Created"
            });

            return ServiceResult<ProductVersionDto>.Created(version.ToDto(), "Product version created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductVersionDto>.InternalServerError($"Error creating product version: {ex.Message}");
        }
    }

    /// <summary>
    /// Tạo Global SKU theo format: EWM-<CAT>-<PID>
    /// Trong đó:
    /// - EWM: Prefix hệ thống (Economy Wood Market)
    /// - CAT: Mã category rút gọn (VD: TB cho Table, WD cho Wood Decor)
    /// - PID: Product_Master ID rút gọn (4 ký tự đầu, VD: 8F3A)
    /// Ví dụ: EWM-TB-8F3A
    /// </summary>
    private async Task<string> GenerateGlobalSkuAsync(Guid productId, Guid categoryId)
    {
        // Prefix hệ thống
        const string prefix = "EWM";

        // Lấy Category để rút gọn tên
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        string categoryCode = "XX"; // Default nếu không tìm thấy category

        if (category != null && !string.IsNullOrEmpty(category.Name))
        {
            // Rút gọn category name thành 2-3 ký tự viết hoa
            // Lấy các chữ cái đầu hoặc 2 ký tự đầu
            var nameParts = category.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 1)
            {
                // Nếu có nhiều từ, lấy chữ cái đầu của mỗi từ (tối đa 3 ký tự)
                categoryCode = string.Concat(nameParts.Take(3).Select(p => p[0])).ToUpper();
            }
            else
            {
                // Nếu chỉ 1 từ, lấy 2-3 ký tự đầu
                categoryCode = category.Name.Length >= 3
                    ? category.Name.Substring(0, 3).ToUpper()
                    : category.Name.ToUpper();
            }
        }

        // Rút gọn ProductId (lấy 4 ký tự đầu)
        string productCode = productId.ToString("N").Substring(0, 4).ToUpper();

        // Tạo Global SKU
        return $"{prefix}-{categoryCode}-{productCode}";
    }

    public async Task<ServiceResult<ProductVersionDto>> UpdateAsync(Guid id, UpdateProductVersionDto dto)
    {
        try
        {
            // Use UnitOfWork repositories to ensure same DbContext for both entities
            var version = await _unitOfWork.ProductVersions.GetByIdAsync(id);
            if (version == null)
                return ServiceResult<ProductVersionDto>.NotFound("Product version not found");

            // Get the ProductMaster to check its status - use UnitOfWork repository
            var product = await _unitOfWork.ProductMasters.GetByIdAsync(version.ProductId);
            if (product == null)
                return ServiceResult<ProductVersionDto>.NotFound("Product not found");

            // Only allow updates if product status is DRAFT or APPROVED
            if (product.Status != ProductStatus.DRAFT && product.Status != ProductStatus.APPROVED)
            {
                return ServiceResult<ProductVersionDto>.BadRequest(
                    $"Cannot update product version. Product must be in DRAFT or APPROVED status. Current status: {product.Status}");
            }

            // Check if product is APPROVED and needs to revert to DRAFT
            bool shouldRevertToDraft = product.Status == ProductStatus.APPROVED;

            // Update version - just modify properties, don't call UpdateAsync
            dto.MapToUpdate(version);
            _unitOfWork.MarkAsModified(version);

            // If product was APPROVED, change it back to DRAFT
            if (shouldRevertToDraft)
            {
                product.Status = ProductStatus.DRAFT;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.MarkAsModified(product);
            }

            // Save all changes - this will save both version and product
            await _unitOfWork.SaveChangesAsync();

            // Get updated data
            var updatedVersion = version.ToDto();

            // Publish event to OrderService with CURRENT product status
            var thumbnailMapUpdated = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
            var thumbnailUrlUpdated = thumbnailMapUpdated.GetValueOrDefault(version.VersionId);
            _eventPublisher.PublishProductVersionUpdated(new ProductVersionUpdatedEvent
            {
                VersionId = version.VersionId,
                ProductId = version.ProductId,
                ShopId = product.ShopId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                ProductStatus = product.Status.ToString(), // This will be DRAFT if shouldRevertToDraft was true
                SellerSku = version.SellerSku,
                VersionNumber = version.VersionNumber,
                VersionName = version.VersionName,
                Price = version.Price,
                Currency = "VND",
                StockQuantity = version.StockQuantity,
                WoodType = version.WoodType,
                WeightGrams = version.WeightGrams,
                LengthCm = version.LengthCm,
                WidthCm = version.WidthCm,
                HeightCm = version.HeightCm,
                IsActive = version.IsActive,
                ThumbnailUrl = thumbnailUrlUpdated,
                UpdatedAt = version.UpdatedAt ?? DateTime.UtcNow,
                EventType = "Updated"
            });

            return ServiceResult<ProductVersionDto>.Success(updatedVersion, "Product version updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductVersionDto>.InternalServerError($"Error updating product version: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeactivateAsync(Guid id)
    {
        try
        {
            var version = await _productVersionRepository.GetByIdAsync(id);
            if (version == null)
                return ServiceResult.NotFound("Product version not found");

            if (!version.IsActive)
                return ServiceResult.BadRequest("Product version is already inactive");

            // Deactivate
            version.IsActive = false;
            version.UpdatedAt = DateTime.UtcNow;

            await _productVersionRepository.UpdateAsync(version);

            // Get product info for event
            var product = await _productMasterRepository.GetByIdAsync(version.ProductId);
            if (product != null)
            {
                // Publish event to OrderService
                var thumbnailMapDeact = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
                var thumbnailUrlDeact = thumbnailMapDeact.GetValueOrDefault(version.VersionId);
                _eventPublisher.PublishProductVersionUpdated(new ProductVersionUpdatedEvent
                {
                    VersionId = version.VersionId,
                    ProductId = version.ProductId,
                    ShopId = product.ShopId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductStatus = product.Status.ToString(),
                    SellerSku = version.SellerSku,
                    VersionNumber = version.VersionNumber,
                    VersionName = version.VersionName,
                    Price = version.Price,
                    Currency = "VND",
                    StockQuantity = version.StockQuantity,
                    WoodType = version.WoodType,
                    WeightGrams = version.WeightGrams,
                    LengthCm = version.LengthCm,
                    WidthCm = version.WidthCm,
                    HeightCm = version.HeightCm,
                    IsActive = version.IsActive,
                    ThumbnailUrl = thumbnailUrlDeact,
                    UpdatedAt = version.UpdatedAt.Value,
                    EventType = "Updated"
                });
            }

            return ServiceResult.Success("Product version deactivated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deactivating product version: {ex.Message}");
        }
    }

    public async Task<ServiceResult> ActivateAsync(Guid id)
    {
        try
        {
            var version = await _productVersionRepository.GetByIdAsync(id);
            if (version == null)
                return ServiceResult.NotFound("Product version not found");

            if (version.IsActive)
                return ServiceResult.BadRequest("Product version is already active");

            // Activate
            version.IsActive = true;
            version.UpdatedAt = DateTime.UtcNow;

            await _productVersionRepository.UpdateAsync(version);

            // Get product info for event
            var product = await _productMasterRepository.GetByIdAsync(version.ProductId);
            if (product != null)
            {
                // Publish event to OrderService
                var thumbnailMapAct = await _imageUrlRepository.GetPrimaryImageBatchAsync("PRODUCT_VERSION", new[] { version.VersionId });
                var thumbnailUrlAct = thumbnailMapAct.GetValueOrDefault(version.VersionId);
                _eventPublisher.PublishProductVersionUpdated(new ProductVersionUpdatedEvent
                {
                    VersionId = version.VersionId,
                    ProductId = version.ProductId,
                    ShopId = product.ShopId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductStatus = product.Status.ToString(),
                    SellerSku = version.SellerSku,
                    VersionNumber = version.VersionNumber,
                    VersionName = version.VersionName,
                    Price = version.Price,
                    Currency = "VND",
                    StockQuantity = version.StockQuantity,
                    WoodType = version.WoodType,
                    WeightGrams = version.WeightGrams,
                    LengthCm = version.LengthCm,
                    WidthCm = version.WidthCm,
                    HeightCm = version.HeightCm,
                    IsActive = version.IsActive,
                    ThumbnailUrl = thumbnailUrlAct,
                    UpdatedAt = version.UpdatedAt.Value,
                    EventType = "Updated"
                });
            }

            return ServiceResult.Success("Product version activated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error activating product version: {ex.Message}");
        }
    }

    public async Task<ServiceResult> SetAsDefaultAsync(Guid id)
    {
        try
        {
            var version = await _productVersionRepository.GetByIdAsync(id);
            if (version == null)
                return ServiceResult.NotFound("Product version not found");

            // Note: IsDefault property has been removed from ProductVersion entity
            // This method is kept for backward compatibility but does nothing now
            // Consider removing this method entirely if not needed

            return ServiceResult.Success("IsDefault property no longer exists. This operation has been deprecated.");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error: {ex.Message}");
        }
    }
}
