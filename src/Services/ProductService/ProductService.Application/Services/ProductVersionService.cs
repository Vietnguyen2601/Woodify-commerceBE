using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ProductService.Application.Services;

public class ProductVersionService : IProductVersionService
{
    private readonly IProductVersionRepository _productVersionRepository;
    private readonly IProductMasterRepository _productMasterRepository;

    public ProductVersionService(
        IProductVersionRepository productVersionRepository,
        IProductMasterRepository productMasterRepository)
    {
        _productVersionRepository = productVersionRepository;
        _productMasterRepository = productMasterRepository;
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

            var version = dto.ToModel();
            await _productVersionRepository.CreateAsync(version);

            return ServiceResult<ProductVersionDto>.Created(version.ToDto(), "Product version created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductVersionDto>.InternalServerError($"Error creating product version: {ex.Message}");
        }
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
