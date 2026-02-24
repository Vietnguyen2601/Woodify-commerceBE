using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
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

    public ProductVersionService(
        IProductVersionRepository productVersionRepository,
        IProductMasterRepository productMasterRepository,
        IUnitOfWork unitOfWork,
        ProductEventPublisher eventPublisher)
    {
        _productVersionRepository = productVersionRepository;
        _productMasterRepository = productMasterRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<ServiceResult<ProductVersionDto>> GetByIdAsync(Guid id)
    {
        var version = await _productVersionRepository.GetByIdAsync(id);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("Product version not found");
        
        return ServiceResult<ProductVersionDto>.Success(version.ToDto());
    }

    public async Task<ServiceResult<ProductVersionDto>> GetBySellerSkuAsync(string sellerSku)
    {
        var version = await _productVersionRepository.GetBySellerSkuAsync(sellerSku);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("Product version not found");
        
        return ServiceResult<ProductVersionDto>.Success(version.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetAllAsync()
    {
        var versions = await _productVersionRepository.GetAllAsync();
        var versionDtos = versions.Select(v => v.ToDto());
        
        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetByProductIdAsync(Guid productId)
    {
        var versions = await _productVersionRepository.GetByProductIdAsync(productId);
        var versionDtos = versions.Select(v => v.ToDto());
        
        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<ProductVersionDto>> GetLatestVersionByProductIdAsync(Guid productId)
    {
        var version = await _productVersionRepository.GetLatestVersionByProductIdAsync(productId);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("No version found for this product");
        
        return ServiceResult<ProductVersionDto>.Success(version.ToDto());
    }

    public async Task<ServiceResult<ProductVersionDto>> GetDefaultVersionByProductIdAsync(Guid productId)
    {
        var version = await _productVersionRepository.GetDefaultVersionByProductIdAsync(productId);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("No default version found for this product");
        
        return ServiceResult<ProductVersionDto>.Success(version.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetInactiveVersionsAsync()
    {
        var versions = await _productVersionRepository.GetInactiveVersionsAsync();
        var versionDtos = versions.Select(v => v.ToDto());
        
        return ServiceResult<IEnumerable<ProductVersionDto>>.Success(versionDtos);
    }

    public async Task<ServiceResult<IEnumerable<ProductVersionDto>>> GetActiveVersionsAsync()
    {
        var versions = await _productVersionRepository.GetActiveVersionsAsync();
        var versionDtos = versions.Select(v => v.ToDto());
        
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

            // Check if product is archived
            if (product.Status == Domain.Entities.ProductStatus.ARCHIVED)
                return ServiceResult<ProductVersionDto>.BadRequest("Cannot create new version for archived product");

            // Check if SellerSku already exists
            var existingSku = await _productVersionRepository.GetBySellerSkuAsync(dto.SellerSku);
            if (existingSku != null)
                return ServiceResult<ProductVersionDto>.BadRequest($"SellerSku '{dto.SellerSku}' already exists");

            // Calculate volume if not provided
            if (!dto.VolumeCm3.HasValue)
            {
                dto.VolumeCm3 = (long)(dto.LengthCm * dto.WidthCm * dto.HeightCm);
            }

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
                ImgUrl = product.ImgUrl,
                Description = product.Description,
                ArAvailable = product.ArAvailable,
                ArModelUrl = product.ArModelUrl,
                Status = product.Status,
                ModerationStatus = product.ModerationStatus,
                ModeratedBy = product.ModeratedBy,
                ModeratedAt = product.ModeratedAt,
                RejectionReason = product.RejectionReason,
                ModerationNotes = product.ModerationNotes,
                AvgRating = product.AvgRating,
                ReviewCount = product.ReviewCount,
                SoldCount = product.SoldCount,
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
            var version = await _productVersionRepository.GetByIdAsync(id);
            if (version == null)
                return ServiceResult<ProductVersionDto>.NotFound("Product version not found");

            // Check if SellerSku is being changed and if it already exists
            if (dto.SellerSku != null && dto.SellerSku != version.SellerSku)
            {
                var existingSku = await _productVersionRepository.GetBySellerSkuAsync(dto.SellerSku);
                if (existingSku != null && existingSku.VersionId != id)
                    return ServiceResult<ProductVersionDto>.BadRequest($"SellerSku '{dto.SellerSku}' already exists");
            }

            dto.MapToUpdate(version);

            // Recalculate volume if dimensions changed
            if (dto.LengthCm.HasValue || dto.WidthCm.HasValue || dto.HeightCm.HasValue)
            {
                version.VolumeCm3 = (long)(version.LengthCm * version.WidthCm * version.HeightCm);
            }

            await _productVersionRepository.UpdateAsync(version);
            
            var updatedVersion = await _productVersionRepository.GetByIdAsync(id);
            return ServiceResult<ProductVersionDto>.Success(updatedVersion!.ToDto(), "Product version updated successfully");
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

            // Get all versions of the product
            var allVersions = await _productVersionRepository.GetByProductIdAsync(version.ProductId);

            // Remove default flag from all versions
            foreach (var v in allVersions)
            {
                v.IsDefault = false;
                v.UpdatedAt = DateTime.UtcNow;
                await _productVersionRepository.UpdateAsync(v);
            }

            // Set current version as default
            version.IsDefault = true;
            version.UpdatedAt = DateTime.UtcNow;
            await _productVersionRepository.UpdateAsync(version);

            return ServiceResult.Success("Product version set as default successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error setting product version as default: {ex.Message}");
        }
    }
}
