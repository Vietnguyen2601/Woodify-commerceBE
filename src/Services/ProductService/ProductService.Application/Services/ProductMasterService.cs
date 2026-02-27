using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Infrastructure.Persistence;
using ProductService.Domain.Parameters;
using Shared.Results;
using Shared.Events;

namespace ProductService.Application.Services;

public class ProductMasterService : IProductMasterService
{
    private readonly IProductMasterRepository _productMasterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductEventPublisher _eventPublisher;

    public ProductMasterService(
        IProductMasterRepository productMasterRepository, 
        IUnitOfWork unitOfWork,
        ProductEventPublisher eventPublisher)
    {
        _productMasterRepository = productMasterRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
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
            // Validate Category exists
            var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId);
            if (!categoryExists)
                return ServiceResult<ProductMasterDto>.NotFound("Category not found");

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

            // Validate Category if being updated
            if (dto.CategoryId.HasValue)
            {
                var categoryExists = await _unitOfWork.Categories.ExistsAsync(dto.CategoryId.Value);
                if (!categoryExists)
                    return ServiceResult<ProductMasterDto>.NotFound("Category not found");
            }

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

            // Publish status change event
            _eventPublisher.PublishProductStatusChanged(new ProductStatusChangedEvent
            {
                ProductId = product.ProductId,
                Status = product.Status.ToString(),
                ChangedAt = DateTime.UtcNow
            });

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

            // Chỉ cho phép publish nếu đã được approved
            if (product.Status != Domain.Entities.ProductStatus.APPROVED)
                return ServiceResult<ProductMasterDto>.BadRequest("Product must be approved before publishing");

            if (product.ModerationStatus != Domain.Entities.ModerationStatus.APPROVED)
                return ServiceResult<ProductMasterDto>.BadRequest("Product must be moderated and approved first");

            product.Status = Domain.Entities.ProductStatus.PUBLISHED;
            product.PublishedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await _productMasterRepository.UpdateAsync(product);

            // Publish status change event
            _eventPublisher.PublishProductStatusChanged(new ProductStatusChangedEvent
            {
                ProductId = product.ProductId,
                Status = product.Status.ToString(),
                ChangedAt = DateTime.UtcNow
            });

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
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                SortBy = searchDto.SortBy,
                SortDirection = searchDto.SortDirection
            };

            var (products, totalCount) = await _productMasterRepository.SearchAsync(searchParams);
            var productDtos = products.Select(p => p.ToDto()).ToList();
            
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

    public async Task<ServiceResult<ProductMasterDto>> SubmitForApprovalAsync(Guid id)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductMasterDto>.NotFound("Product not found");

            if (product.Status != Domain.Entities.ProductStatus.DRAFT)
                return ServiceResult<ProductMasterDto>.BadRequest("Only draft products can be submitted for approval");

            product.Status = Domain.Entities.ProductStatus.PENDING_APPROVAL;
            product.ModerationStatus = Domain.Entities.ModerationStatus.PENDING;
            product.UpdatedAt = DateTime.UtcNow;
            await _productMasterRepository.UpdateAsync(product);

            return ServiceResult<ProductMasterDto>.Success(product.ToDto(), "Product submitted for approval successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error submitting product for approval: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductMasterDto>> ModerateProductAsync(Guid id, ModerateProductDto dto)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductMasterDto>.NotFound("Product not found");

            if (product.Status != Domain.Entities.ProductStatus.PENDING_APPROVAL)
                return ServiceResult<ProductMasterDto>.BadRequest("Only products pending approval can be moderated");

            dto.MapToModerate(product);
            await _productMasterRepository.UpdateAsync(product);

            var message = dto.ModerationStatus == Domain.Entities.ModerationStatus.APPROVED 
                ? "Product approved successfully" 
                : "Product rejected";

            return ServiceResult<ProductMasterDto>.Success(product.ToDto(), message);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDto>.InternalServerError($"Error moderating product: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<ProductMasterDto>>> GetPendingApprovalProductsAsync()
    {
        try
        {
            var pendingProducts = await _productMasterRepository.GetByStatusAsync(Domain.Entities.ProductStatus.PENDING_APPROVAL);
            var productDtos = pendingProducts.Select(p => p.ToDto());
            
            return ServiceResult<IEnumerable<ProductMasterDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ProductMasterDto>>.InternalServerError($"Error retrieving pending approval products: {ex.Message}");
        }
    }
}
