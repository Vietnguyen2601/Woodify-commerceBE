using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ProductService.Infrastructure.Data.Context;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    // Constructor mặc định cho design-time (migrations)
    public ProductDbContext() : base()
    {
    }

    public DbSet<ProductMaster> ProductMasters { get; set; }
    public DbSet<ProductVersion> ProductVersions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    private static string GetConnectionString(string connectionStringName)
    {
        var rootEnvPath = FindEnvFile();
        if (rootEnvPath != null && File.Exists(rootEnvPath))
        {
            foreach (var line in File.ReadAllLines(rootEnvPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }

        // Kiểm tra biến môi trường trước
        string envConnectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return config.GetConnectionString(connectionStringName);
    }

    private static string FindEnvFile()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            var envFile = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envFile))
                return envFile;
            dir = dir.Parent;
        }
        return null;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString("ProductService"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // Cấu hình bảng Product_Master
        // ========================================
        modelBuilder.Entity<ProductMaster>(entity =>
        {
            entity.ToTable("product_master");
            entity.HasKey(e => e.ProductId);
            
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.GlobalSku).HasColumnName("global_sku").HasMaxLength(255);
            
            entity.Property(e => e.ImgUrl).HasColumnName("img_url");
            entity.Property(e => e.Description).HasColumnName("description");
            
            entity.Property(e => e.ArAvailable).HasColumnName("ar_available").HasDefaultValue(false);
            entity.Property(e => e.ArModelUrl).HasColumnName("ar_model_url");
            
            entity.Property(e => e.AvgRating)
                .HasColumnName("avg_rating")
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0);
            entity.Property(e => e.ReviewCount)
                .HasColumnName("review_count")
                .HasDefaultValue(0);
            entity.Property(e => e.SoldCount)
                .HasColumnName("sold_count")
                .HasDefaultValue(0);
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasDefaultValue(ProductStatus.DRAFT)
                .IsRequired();
            
            // Moderation fields
            entity.Property(e => e.ModerationStatus)
                .HasColumnName("moderation_status")
                .HasConversion<string>()
                .HasDefaultValue(ModerationStatus.PENDING)
                .IsRequired();
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.ModeratedAt).HasColumnName("moderated_at");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.ModerationNotes).HasColumnName("moderation_notes");
            
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");

            // Foreign Key to Category
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.GlobalSku).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ModerationStatus);
        });

        // ========================================
        // Cấu hình bảng Product_Version
        // ========================================
        modelBuilder.Entity<ProductVersion>(entity =>
        {
            entity.ToTable("product_version");
            entity.HasKey(e => e.VersionId);
            
            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.SellerSku).HasColumnName("seller_sku").IsRequired().HasMaxLength(255);
            entity.Property(e => e.VersionNumber).HasColumnName("version_number").HasDefaultValue(1);
            entity.Property(e => e.VersionName).HasColumnName("version_name").HasMaxLength(500);
            entity.Property(e => e.PriceCents).HasColumnName("price_cents").IsRequired();
            entity.Property(e => e.BasePriceCents).HasColumnName("base_price_cents");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            entity.Property(e => e.LowStockThreshold).HasColumnName("low_stock_threshold").HasDefaultValue(5);
            entity.Property(e => e.AllowBackorder).HasColumnName("allow_backorder").HasDefaultValue(false);
            entity.Property(e => e.WeightGrams).HasColumnName("weight_grams").IsRequired();
            entity.Property(e => e.LengthCm).HasColumnName("length_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.WidthCm).HasColumnName("width_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.VolumeCm3).HasColumnName("volume_cm3");
            entity.Property(e => e.BulkyType).HasColumnName("bulky_type").HasMaxLength(50);
            entity.Property(e => e.IsFragile).HasColumnName("is_fragile").HasDefaultValue(false);
            entity.Property(e => e.RequiresSpecialHandling).HasColumnName("requires_special_handling").HasDefaultValue(false);
            entity.Property(e => e.WarrantyMonths).HasColumnName("warranty_months").HasDefaultValue(12);
            entity.Property(e => e.WarrantyTerms).HasColumnName("warranty_terms");
            entity.Property(e => e.IsBundle).HasColumnName("is_bundle").HasDefaultValue(false);
            entity.Property(e => e.BundleDiscountCents).HasColumnName("bundle_discount_cents").HasDefaultValue(0);
            entity.Property(e => e.PrimaryImageUrl).HasColumnName("primary_image_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.SellerSku).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ========================================
        // Cấu hình bảng Category
        // ========================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Level).HasColumnName("level").HasDefaultValue(0);
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Self-referencing relationship
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.ParentCategoryId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // ========================================
        // Cấu hình bảng Product_Review
        // ========================================
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("product_review");
            entity.HasKey(e => e.ReviewId);
            
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(5000);
            entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
            entity.Property(e => e.HiddenBy).HasColumnName("hidden_by");
            entity.Property(e => e.HiddenAt).HasColumnName("hidden_at");
            entity.Property(e => e.ShopResponse).HasColumnName("shop_response").HasMaxLength(5000);
            entity.Property(e => e.ShopResponseAt).HasColumnName("shop_response_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Foreign Keys
            entity.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(r => r.Version)
                .WithMany()
                .HasForeignKey(r => r.VersionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.VersionId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.IsVisible);
            entity.HasIndex(e => e.CreatedAt);
            
            // Unique constraint: một user chỉ review một order một lần
            entity.HasIndex(e => new { e.OrderId, e.AccountId }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tự động cập nhật UpdatedAt khi save
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is ProductMaster product)
                product.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is ProductVersion version)
                version.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Category category)
                category.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is ProductReview review)
                review.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
