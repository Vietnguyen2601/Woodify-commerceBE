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

            // Indexes
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.GlobalSku).IsUnique();
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
            entity.HasIndex(e => e.Sku).IsUnique();
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
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
