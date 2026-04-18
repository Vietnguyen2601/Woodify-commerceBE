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
    public DbSet<ImageUrl> ImageUrls { get; set; }
    public DbSet<ReviewPurchaseEligibility> ReviewPurchaseEligibilities { get; set; }
    public DbSet<OrderDeliveredStockLedger> OrderDeliveredStockLedgers { get; set; }
    public DbSet<ShopRegistryEntry> ShopRegistry { get; set; }
    public DbSet<OrderProductMirror> OrderProductMirrors { get; set; }

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
        // Cấu hình bảng Shop_Registry (không gọi HTTP)
        // ========================================
        modelBuilder.Entity<ShopRegistryEntry>(entity =>
        {
            entity.ToTable("shop_registry");
            entity.HasKey(e => e.ShopId);

            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Name);
        });

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

            entity.Property(e => e.Description).HasColumnName("description");

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
            entity.Property(e => e.ModeratedAt).HasColumnName("moderated_at");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.AverageRating).HasColumnName("average_rating").HasColumnType("double precision");
            entity.Property(e => e.ReviewCount).HasColumnName("review_count").HasDefaultValue(0);
            entity.Property(e => e.Sales).HasColumnName("sales").HasDefaultValue(0);

            // Foreign Key to Category
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
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
        // Cấu hình bảng Product_Versions
        // ========================================
        modelBuilder.Entity<ProductVersion>(entity =>
        {
            entity.ToTable("product_versions");
            entity.HasKey(e => e.VersionId);

            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.SellerSku).HasColumnName("seller_sku").IsRequired().HasMaxLength(255);
            entity.Property(e => e.VersionNumber).HasColumnName("version_number");
            entity.Property(e => e.VersionName).HasColumnName("version_name").HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnName("price").IsRequired();
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            entity.Property(e => e.WoodType).HasColumnName("wood_type").HasMaxLength(200);
            entity.Property(e => e.WeightGrams).HasColumnName("weight_grams").IsRequired();
            entity.Property(e => e.LengthCm).HasColumnName("length_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.WidthCm).HasColumnName("width_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Versions)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.SellerSku).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ========================================
        // Cấu hình bảng Categories
        // ========================================
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.Level).HasColumnName("level").HasDefaultValue(0);
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
        });

        // ========================================
        // Cấu hình bảng Product_Reviews
        // ========================================
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("product_reviews");
            entity.HasKey(e => e.ReviewId);

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.VersionId).HasColumnName("version_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(5000);
            entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
            entity.Property(e => e.ShopResponse).HasColumnName("shop_response").HasMaxLength(5000);
            entity.Property(e => e.ShopResponseAt).HasColumnName("shop_response_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Foreign Keys
            entity.HasOne(r => r.Version)
                .WithMany(v => v.Reviews)
                .HasForeignKey(r => r.VersionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.VersionId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.IsVisible);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasIndex(e => new { e.OrderItemId, e.AccountId }).IsUnique();
        });

        modelBuilder.Entity<ReviewPurchaseEligibility>(entity =>
        {
            entity.ToTable("review_purchase_eligibility");
            entity.HasKey(e => e.OrderItemId);
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.VersionId).HasColumnName("version_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            entity.Property(e => e.EligibleAt).HasColumnName("eligible_at").IsRequired();
            entity.Property(e => e.IsConsumed).HasColumnName("is_consumed").HasDefaultValue(false);
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
        });

        modelBuilder.Entity<OrderDeliveredStockLedger>(entity =>
        {
            entity.ToTable("order_delivered_stock_ledger");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at").IsRequired();
        });

        modelBuilder.Entity<OrderProductMirror>(entity =>
        {
            entity.ToTable("order_product_mirror");
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(32);
            entity.Property(e => e.SubtotalVnd).HasColumnName("subtotal_vnd");
            entity.Property(e => e.TotalAmountVnd).HasColumnName("total_amount_vnd");
            entity.Property(e => e.CommissionVnd).HasColumnName("commission_vnd");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate").HasPrecision(18, 6);
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.DeliveryAddress).HasColumnName("delivery_address");
            entity.Property(e => e.ProviderServiceCode).HasColumnName("provider_service_code").HasMaxLength(32);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.LastSnapshotAt).HasColumnName("last_snapshot_at").IsRequired();
            entity.Property(e => e.LineItemsJson).HasColumnName("line_items_json").HasColumnType("jsonb");

            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastSnapshotAt);
        });

        // ========================================
        // Cấu hình bảng image_urls
        // ========================================
        modelBuilder.Entity<ImageUrl>(entity =>
        {
            entity.ToTable("image_urls");
            entity.HasKey(e => e.ImageId);

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImageType).HasColumnName("image_type").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id").IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            entity.Property(e => e.OriginalUrl).HasColumnName("original_url").IsRequired();
            entity.Property(e => e.PublicId).HasColumnName("public_id").HasMaxLength(512);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            entity.HasIndex(e => new { e.ImageType, e.ReferenceId });
            entity.HasIndex(e => e.ReferenceId);
            entity.HasIndex(e => e.CreatedAt);
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
