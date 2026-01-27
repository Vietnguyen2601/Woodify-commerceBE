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
            entity.Property(e => e.GlobalSku).HasColumnName("global_sku").HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();
            entity.Property(e => e.Certified).HasColumnName("certified").IsRequired();
            entity.Property(e => e.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(e => e.AvgRating)
                .HasColumnName("avg_rating")
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0);
            entity.Property(e => e.ReviewCount)
                .HasColumnName("review_count")
                .HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Foreign Key to Category
            entity.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.CategoryId);
            // Remove unique constraint to allow multiple null values during product creation
            // GlobalSku will be generated when first version is created
            entity.HasIndex(e => e.GlobalSku); 
            entity.HasIndex(e => e.Status);
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
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
            entity.Property(e => e.PriceCents).HasColumnName("price_cents");
            entity.Property(e => e.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10);
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(255);
            entity.Property(e => e.ArAvailable).HasColumnName("ar_available").IsRequired();
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Relationships
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            // Bỏ unique constraint cho SKU vì có thể nhiều versions có cùng SKU hoặc null
            entity.HasIndex(e => e.Sku); 
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");

            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");

            entity.Property(e => e.ParentCategoryId)
                .HasColumnName("parent_category_id");

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Self-referencing relationship
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ParentCategoryId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
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
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(500);
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(5000);
            entity.Property(e => e.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
            entity.Property(e => e.HelpfulCount).HasColumnName("helpful_count").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Foreign Key to Product
            entity.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.IsVerified);
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
