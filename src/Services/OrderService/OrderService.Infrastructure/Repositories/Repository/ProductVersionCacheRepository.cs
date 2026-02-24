using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Infrastructure.Repositories.IRepositories;
using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Repositories.Repository;

public class ProductVersionCacheRepository : GenericRepository<ProductVersionCache>, IProductVersionCacheRepository
{
    public ProductVersionCacheRepository(OrderDbContext context) : base(context)
    {
    }

    public async Task<ProductVersionCache?> GetByVersionIdAsync(Guid versionId)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.VersionId == versionId);
    }

    public async Task<List<ProductVersionCache>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet.Where(p => p.ProductId == productId).ToListAsync();
    }

    public async Task UpsertAsync(ProductVersionCache cache)
    {
        var existing = await GetByVersionIdAsync(cache.VersionId);
        if (existing != null)
        {
            // Update all fields
            existing.ProductId = cache.ProductId;
            existing.ShopId = cache.ShopId;
            
            // Product Master Info
            existing.ProductName = cache.ProductName;
            existing.ProductDescription = cache.ProductDescription;
            existing.ProductStatus = cache.ProductStatus;
            
            // Version Info
            existing.SellerSku = cache.SellerSku;
            existing.VersionNumber = cache.VersionNumber;
            existing.VersionName = cache.VersionName;
            
            // Pricing
            existing.PriceCents = cache.PriceCents;
            existing.BasePriceCents = cache.BasePriceCents;
            existing.Currency = cache.Currency;
            
            // Stock
            existing.StockQuantity = cache.StockQuantity;
            existing.LowStockThreshold = cache.LowStockThreshold;
            existing.AllowBackorder = cache.AllowBackorder;
            
            // Shipping Dimensions
            existing.WeightGrams = cache.WeightGrams;
            existing.LengthCm = cache.LengthCm;
            existing.WidthCm = cache.WidthCm;
            existing.HeightCm = cache.HeightCm;
            existing.VolumeCm3 = cache.VolumeCm3;
            
            // Shipping Properties
            existing.BulkyType = cache.BulkyType;
            existing.IsFragile = cache.IsFragile;
            existing.RequiresSpecialHandling = cache.RequiresSpecialHandling;
            
            // Warranty
            existing.WarrantyMonths = cache.WarrantyMonths;
            existing.WarrantyTerms = cache.WarrantyTerms;
            
            // Bundle
            existing.IsBundle = cache.IsBundle;
            existing.BundleDiscountCents = cache.BundleDiscountCents;
            
            // Images
            existing.PrimaryImageUrl = cache.PrimaryImageUrl;
            
            // Status
            existing.IsActive = cache.IsActive;
            existing.IsDefault = cache.IsDefault;
            
            existing.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(existing);
        }
        else
        {
            cache.LastUpdated = DateTime.UtcNow;
            await CreateAsync(cache);
        }
    }

    public async Task UpdateProductStatusAsync(Guid productId, string status)
    {
        var versions = await GetByProductIdAsync(productId);
        foreach (var version in versions)
        {
            version.ProductStatus = status;
            version.LastUpdated = DateTime.UtcNow;
            await UpdateAsync(version);
        }
    }
}
