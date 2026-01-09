using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace ProductService.Application.Services;

public class ProductMasterService : IProductMasterService
{
    private readonly IProductMasterRepository _productMasterRepository;

    public ProductMasterService(IProductMasterRepository productMasterRepository)
    {
        _productMasterRepository = productMasterRepository;
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
        var products = await _productMasterRepository.GetAllAsync();
        var productDtos = products.Select(p => p.ToDto());
        
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
            var product = dto.ToModel();
            await _productMasterRepository.CreateAsync(product);

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

            dto.MapToUpdate(product);
            await _productMasterRepository.UpdateAsync(product);
            
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
            return ServiceResult.Success("Product deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting product: {ex.Message}");
        }
    }
}
