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
    private readonly IProductVersionRepository _productVersionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProductEventPublisher _eventPublisher;

    public ProductMasterService(
        IProductMasterRepository productMasterRepository,
        IProductVersionRepository productVersionRepository,
        IUnitOfWork unitOfWork,
        ProductEventPublisher eventPublisher)
    {
        _productMasterRepository = productMasterRepository;
        _productVersionRepository = productVersionRepository;
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

            // Store current status for business logic
            var currentStatus = product.Status;

            // PENDING_APPROVAL: Block all edits to avoid review drift
            if (currentStatus == Domain.Entities.ProductStatus.PENDING_APPROVAL)
            {
                return ServiceResult<ProductMasterDto>.BadRequest(
                    "Cannot edit product while it is pending approval. Please wait for review or cancel the approval request.");
            }

            // Track if buyer-facing content has changed
            bool buyerFacingContentChanged = false;

            // Check for changes in buyer-facing content (name, description, category)
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != product.Name)
                buyerFacingContentChanged = true;
            
            if (dto.Description != null && dto.Description != product.Description)
                buyerFacingContentChanged = true;
            
            if (dto.CategoryId.HasValue && dto.CategoryId.Value != product.CategoryId)
                buyerFacingContentChanged = true;

            // Determine if we need to reset to PENDING_APPROVAL
            bool requiresReModeration = false;
            
            // APPROVED: Allow edits but reset to PENDING_APPROVAL if buyer-facing content changes
            if (currentStatus == Domain.Entities.ProductStatus.APPROVED && buyerFacingContentChanged)
            {
                requiresReModeration = true;
            }

            // PUBLISHED: Limited edits - require re-moderation for buyer-facing content changes
            if (currentStatus == Domain.Entities.ProductStatus.PUBLISHED && buyerFacingContentChanged)
            {
                requiresReModeration = true;
            }

            // DRAFT, REJECTED: Editable freely - no special logic needed

            // Apply the updates from DTO first
            dto.MapToUpdate(product);

            // Override status and moderation if re-moderation is required
            if (requiresReModeration)
            {
                product.Status = Domain.Entities.ProductStatus.PENDING_APPROVAL;
                product.ModerationStatus = Domain.Entities.ModerationStatus.PENDING;
                product.ModeratedAt = null;
                
                // Unpublish if was PUBLISHED
                if (currentStatus == Domain.Entities.ProductStatus.PUBLISHED)
                {
                    product.PublishedAt = null;
                }
            }

            // Save changes
            await _productMasterRepository.UpdateAsync(product);
            
            var updatedProduct = await _productMasterRepository.GetByIdAsync(id);
            
            string message = "Product updated successfully";
            if (requiresReModeration)
            {
                message = "Product updated successfully. Status changed to PENDING_APPROVAL due to changes in buyer-facing content (name/description/category). Re-moderation required.";
            }

            return ServiceResult<ProductMasterDto>.Success(updatedProduct!.ToDto(), message);
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
            
            // Soft delete: Set status to DELETED and update timestamp
            product.Status = Domain.Entities.ProductStatus.DELETED;
            product.UpdatedAt = DateTime.UtcNow;
            
            await _productMasterRepository.UpdateAsync(product);
            
            // Publish event để thông báo cho OrderService
            _eventPublisher.PublishProductDeleted(new ProductDeletedEvent
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                ProductName = product.Name,
                DeletedAt = product.UpdatedAt.Value,
                EventType = "ProductDeleted"
            });
            
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

            // Allow submission from DRAFT or REJECTED status
            if (product.Status != Domain.Entities.ProductStatus.DRAFT && 
                product.Status != Domain.Entities.ProductStatus.REJECTED)
                return ServiceResult<ProductMasterDto>.BadRequest("Only draft or rejected products can be submitted for approval");

            // Pre-submit validation: Check if product has at least 1 active version
            var versions = await _productVersionRepository.GetByProductIdAsync(id);
            var activeVersions = versions.Where(v => v.IsActive).ToList();
            
            if (!activeVersions.Any())
                return ServiceResult<ProductMasterDto>.BadRequest("Product must have at least 1 active version before submission");

            // Transition to PENDING_APPROVAL
            product.Status = Domain.Entities.ProductStatus.PENDING_APPROVAL;
            product.ModerationStatus = Domain.Entities.ModerationStatus.PENDING;
            
            // Clear previous rejection/moderation fields
            product.ModeratedAt = null;
            
            product.UpdatedAt = DateTime.UtcNow;
            await _productMasterRepository.UpdateAsync(product);

            // Publish status change event
            _eventPublisher.PublishProductStatusChanged(new ProductStatusChangedEvent
            {
                ProductId = product.ProductId,
                Status = product.Status.ToString(),
                ChangedAt = DateTime.UtcNow
            });

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

            // Publish status change event
            _eventPublisher.PublishProductStatusChanged(new ProductStatusChangedEvent
            {
                ProductId = product.ProductId,
                Status = product.Status.ToString(),
                ChangedAt = DateTime.UtcNow
            });

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

    /// <summary>
    /// Get product detail with versions
    /// For seller/admin: Returns all information including status and moderation_status
    /// For buyer: Only returns PUBLISHED products with active versions
    /// </summary>
    public async Task<ServiceResult<ProductMasterDetailDto>> GetProductDetailAsync(Guid productId, string userRole)
    {
        try
        {
            var product = await _productMasterRepository.GetByIdAsync(productId);
            if (product == null)
                return ServiceResult<ProductMasterDetailDto>.NotFound("Product not found");

            // Role-based filtering
            bool isSellerOrAdmin = userRole?.ToLower() is "seller" or "admin";

            // For buyer, only show PUBLISHED products
            if (!isSellerOrAdmin && product.Status != Domain.Entities.ProductStatus.PUBLISHED)
            {
                return ServiceResult<ProductMasterDetailDto>.NotFound("Product not found");
            }

            // Get all versions for this product
            var versions = await _productVersionRepository.GetByProductIdAsync(productId);

            // Filter versions based on role
            var filteredVersions = isSellerOrAdmin 
                ? versions 
                : versions.Where(v => v.IsActive).ToList();

            // Get category name
            var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

            // Map to detail DTO
            var detailDto = new ProductMasterDetailDto
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                CategoryId = product.CategoryId,
                CategoryName = category?.Name,
                Name = product.Name,
                GlobalSku = product.GlobalSku,
                Description = product.Description,
                Status = product.Status.ToString(),
                ModerationStatus = isSellerOrAdmin ? product.ModerationStatus.ToString() : null,
                ModeratedAt = isSellerOrAdmin ? product.ModeratedAt : null,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                PublishedAt = product.PublishedAt,
                Versions = filteredVersions.Select(v => new ProductVersionDetailDto
                {
                    VersionId = v.VersionId,
                    VersionNumber = v.VersionNumber,
                    VersionName = v.VersionName,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    WoodType = v.WoodType,
                    WeightGrams = v.WeightGrams,
                    LengthCm = v.LengthCm,
                    WidthCm = v.WidthCm,
                    HeightCm = v.HeightCm,
                    IsActive = v.IsActive,
                    CreatedAt = v.CreatedAt
                }).ToList()
            };

            return ServiceResult<ProductMasterDetailDto>.Success(detailDto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductMasterDetailDto>.InternalServerError($"Error retrieving product detail: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all products with versions (paginated)
    /// For seller/admin: Returns all products with all information
    /// For buyer: Only returns PUBLISHED products with active versions
    /// </summary>
    public async Task<ServiceResult<ProductDetailListResultDto>> GetAllProductDetailsAsync(
        string userRole, 
        int page = 1, 
        int pageSize = 20, 
        Guid? shopId = null, 
        Guid? categoryId = null)
    {
        try
        {
            // Validate pagination
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            bool isSellerOrAdmin = userRole?.ToLower() is "seller" or "admin";

            // Get products based on role and filters
            IEnumerable<Domain.Entities.ProductMaster> products;
            
            if (shopId.HasValue)
            {
                products = await _productMasterRepository.GetByShopIdAsync(shopId.Value);
            }
            else
            {
                products = await _productMasterRepository.GetAllAsync();
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // For buyer, filter to only PUBLISHED products
            if (!isSellerOrAdmin)
            {
                products = products.Where(p => p.Status == Domain.Entities.ProductStatus.PUBLISHED);
            }

            // Get total count before pagination
            var totalCount = products.Count();

            // Apply pagination
            var pagedProducts = products
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get all categories (for performance, get once)
            var categoryIds = pagedProducts.Select(p => p.CategoryId).Distinct().ToList();
            var categories = new Dictionary<Guid, string>();
            foreach (var catId in categoryIds)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(catId);
                if (category != null)
                {
                    categories[catId] = category.Name;
                }
            }

            // Build product details with versions
            var productDetails = new List<ProductMasterDetailDto>();
            foreach (var product in pagedProducts)
            {
                // Get versions for this product
                var versions = await _productVersionRepository.GetByProductIdAsync(product.ProductId);

                // Filter versions based on role
                var filteredVersions = isSellerOrAdmin 
                    ? versions 
                    : versions.Where(v => v.IsActive).ToList();

                var detailDto = new ProductMasterDetailDto
                {
                    ProductId = product.ProductId,
                    ShopId = product.ShopId,
                    CategoryId = product.CategoryId,
                    CategoryName = categories.ContainsKey(product.CategoryId) ? categories[product.CategoryId] : null,
                    Name = product.Name,
                    GlobalSku = product.GlobalSku,
                    Description = product.Description,
                    Status = product.Status.ToString(),
                    ModerationStatus = isSellerOrAdmin ? product.ModerationStatus.ToString() : null,
                    ModeratedAt = isSellerOrAdmin ? product.ModeratedAt : null,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    PublishedAt = product.PublishedAt,
                    Versions = filteredVersions.Select(v => new ProductVersionDetailDto
                    {
                        VersionId = v.VersionId,
                        VersionNumber = v.VersionNumber,
                        VersionName = v.VersionName,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        WoodType = v.WoodType,
                        WeightGrams = v.WeightGrams,
                        LengthCm = v.LengthCm,
                        WidthCm = v.WidthCm,
                        HeightCm = v.HeightCm,
                        IsActive = v.IsActive,
                        CreatedAt = v.CreatedAt
                    }).ToList()
                };

                productDetails.Add(detailDto);
            }

            var result = new ProductDetailListResultDto
            {
                Products = productDetails,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<ProductDetailListResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<ProductDetailListResultDto>.InternalServerError($"Error retrieving product details: {ex.Message}");
        }
    }
}
