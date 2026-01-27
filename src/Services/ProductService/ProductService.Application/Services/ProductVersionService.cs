using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using Shared.Results;

namespace ProductService.Application.Services;

public class ProductVersionService : IProductVersionService
{
    private readonly IProductVersionRepository _productVersionRepository;
    private readonly IProductMasterRepository _productMasterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductVersionService(
        IProductVersionRepository productVersionRepository,
        IProductMasterRepository productMasterRepository,
        IUnitOfWork unitOfWork)
    {
        _productVersionRepository = productVersionRepository;
        _productMasterRepository = productMasterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<ProductVersionDto>> GetByIdAsync(Guid id)
    {
        var version = await _productVersionRepository.GetByIdAsync(id);
        if (version == null)
            return ServiceResult<ProductVersionDto>.NotFound("Product version not found");
        
        return ServiceResult<ProductVersionDto>.Success(version.ToDto());
    }

    public async Task<ServiceResult<ProductVersionDto>> GetBySkuAsync(string sku)
    {
        var version = await _productVersionRepository.GetBySkuAsync(sku);
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
                GlobalSku = product.GlobalSku, // Keep existing GlobalSku by default
                Status = product.Status,
                Certified = product.Certified,
                CurrentVersionId = version.VersionId,
                AvgRating = product.AvgRating,
                ReviewCount = product.ReviewCount,
                CreatedAt = product.CreatedAt,
                UpdatedAt = DateTime.UtcNow
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

            dto.MapToUpdate(version);
            await _productVersionRepository.UpdateAsync(version);
            
            var updatedVersion = await _productVersionRepository.GetByIdAsync(id);
            return ServiceResult<ProductVersionDto>.Success(updatedVersion!.ToDto(), "Product version updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductVersionDto>.InternalServerError($"Error updating product version: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var version = await _productVersionRepository.GetByIdAsync(id);
            if (version == null)
                return ServiceResult.NotFound("Product version not found");
            
            await _productVersionRepository.RemoveAsync(version);
            return ServiceResult.Success("Product version deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting product version: {ex.Message}");
        }
    }
}
