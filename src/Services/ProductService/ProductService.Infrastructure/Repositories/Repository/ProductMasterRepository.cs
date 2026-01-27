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
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<ProductMaster?> GetByGlobalSkuAsync(string globalSku)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.GlobalSku == globalSku);
    }

    public async Task<List<ProductMaster>> GetByShopIdAsync(Guid shopId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.ShopId == shopId)
            .ToListAsync();
    }

    public async Task<List<ProductMaster>> GetByStatusAsync(ProductStatus status)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    public async Task<(List<ProductMaster> Products, int TotalCount)> SearchAsync(ProductSearchParameters searchParams)
    {
        var query = _context.ProductMasters
            .Include(p => p.Category)
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

        // Filter by Rating range
        if (searchParams.MinRating.HasValue)
        {
            query = query.Where(p => p.AvgRating >= (decimal)searchParams.MinRating.Value);
        }
        if (searchParams.MaxRating.HasValue)
        {
            query = query.Where(p => p.AvgRating <= (decimal)searchParams.MaxRating.Value);
        }

        // Search by keyword in ProductVersion (title, description)
        if (!string.IsNullOrWhiteSpace(searchParams.Keyword))
        {
            var keyword = $"%{searchParams.Keyword}%";
            query = query.Where(p =>
                _context.ProductVersions
                    .Where(v => v.ProductId == p.ProductId)
                    .Any(v => EF.Functions.ILike(v.Title, keyword) ||
                             (v.Description != null && EF.Functions.ILike(v.Description, keyword)))
            );
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Sorting
        query = searchParams.SortBy.ToLower() switch
        {
            "avgrating" => searchParams.SortDirection.ToUpper() == "ASC"
                ? query.OrderBy(p => p.AvgRating)
                : query.OrderByDescending(p => p.AvgRating),
            "name" => searchParams.SortDirection.ToUpper() == "ASC"
                ? query.OrderBy(p => p.Category!.Name)
                : query.OrderByDescending(p => p.Category!.Name),
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

    public override async Task<List<ProductMaster>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .ToListAsync();
    }

    public override async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(p => p.ProductId == id);
    }
}
