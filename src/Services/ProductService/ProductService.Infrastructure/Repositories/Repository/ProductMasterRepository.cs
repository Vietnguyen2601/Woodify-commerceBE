using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Repositories.Base;
using ProductService.Infrastructure.Repositories.IRepositories;
using ProductService.Domain.Entities;
using ProductService.Domain.Parameters;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Repositories;

public class ProductMasterRepository : GenericRepository<ProductMaster>, IProductMasterRepository
{
    public ProductMasterRepository(ProductDbContext context) : base(context)
    {
    }

    public override async Task<ProductMaster?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<ProductMaster?> GetByGlobalSkuAsync(string globalSku)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.GlobalSku == globalSku);
    }

    public async Task<List<ProductMaster>> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .Where(p => p.ShopId == shopId)
            .ToListAsync();
    }

    public async Task<List<ProductMaster>> GetByStatusAsync(ProductStatus status)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<(List<ProductMaster> Products, int TotalCount)> SearchAsync(ProductSearchParameters searchParams)
    {
        var query = _context.ProductMasters
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .AsQueryable();

        // Default: Only index PUBLISHED products
        var status = Enum.Parse<ProductStatus>(searchParams.Status, true);
        query = query.Where(p => p.Status == status);

        // Filter by Category name
        if (!string.IsNullOrWhiteSpace(searchParams.CategoryName))
        {
            var categoryPattern = $"%{searchParams.CategoryName}%";
            query = query.Where(p => EF.Functions.ILike(p.Category!.Name, categoryPattern));
        }

        // Rating filter removed as AvgRating is no longer in ProductMaster
        // Ratings are now managed separately through ProductReviews

        // Search by keyword in ProductVersion (VersionName, SellerSku) and ProductMaster
        if (!string.IsNullOrWhiteSpace(searchParams.Keyword))
        {
            var keyword = $"%{searchParams.Keyword}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, keyword) ||
                (p.Description != null && EF.Functions.ILike(p.Description, keyword)) ||
                _context.ProductVersions
                    .Where(v => v.ProductId == p.ProductId)
                    .Any(v => (v.VersionName != null && EF.Functions.ILike(v.VersionName, keyword)) ||
                             EF.Functions.ILike(v.SellerSku, keyword))
            );
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Sorting
        query = searchParams.SortBy.ToLower() switch
        {
            "name" => searchParams.SortDirection.ToUpper() == "ASC"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            "publishedat" => searchParams.SortDirection.ToUpper() == "ASC"
                ? query.OrderBy(p => p.PublishedAt)
                : query.OrderByDescending(p => p.PublishedAt),
            _ => searchParams.SortDirection.ToUpper() == "ASC"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };

        // Pagination
        var products = await query
            .Skip((searchParams.Page - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<(List<ProductMaster> Products, int TotalCount)> GetPendingApprovalQueueAsync(
        Guid? categoryId = null, 
        Guid? shopId = null, 
        DateTime? submittedFrom = null, 
        DateTime? submittedTo = null,
        int page = 1,
        int pageSize = 20)
    {
        // Base query: status=PENDING_APPROVAL and moderation_status=PENDING
        var query = _context.ProductMasters
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .Where(p => p.Status == ProductStatus.PENDING_APPROVAL && 
                       p.ModerationStatus == ModerationStatus.PENDING)
            .AsQueryable();

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Filter by shop
        if (shopId.HasValue)
        {
            query = query.Where(p => p.ShopId == shopId.Value);
        }

        // Filter by submission time range
        // UpdatedAt represents the last update time, which is when the product was submitted for approval
        if (submittedFrom.HasValue)
        {
            query = query.Where(p => p.UpdatedAt >= submittedFrom.Value);
        }

        if (submittedTo.HasValue)
        {
            query = query.Where(p => p.UpdatedAt <= submittedTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Sort by oldest first (earliest submission)
        query = query.OrderBy(p => p.UpdatedAt);

        // Pagination
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public override async Task<List<ProductMaster>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Versions)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(p => p.ProductId == id);
    }
}
