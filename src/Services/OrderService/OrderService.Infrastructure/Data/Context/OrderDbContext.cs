using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data.Context;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<ProductVersionCache> ProductVersionCaches { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionString("OrderService");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }

    private static string? GetConnectionString(string connectionStringName)
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

        // Check environment variable first
        string? envConnectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");
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

    private static string? FindEnvFile()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, ".env")))
                return Path.Combine(dir.FullName, ".env");
            dir = dir.Parent;
        }
        return null;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // Cấu hình bảng Carts
        // ========================================
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts");
            entity.HasKey(e => e.CartId);
            
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            // Index for faster lookups
            entity.HasIndex(e => e.AccountId);
        });

        // ========================================
        // Cấu hình bảng Cart_Items
        // ========================================
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.CartItemId);
            
            entity.Property(e => e.CartItemId).HasColumnName("cart_item_id");
            entity.Property(e => e.CartId).HasColumnName("cart_id").IsRequired();
            entity.Property(e => e.VersionId).HasColumnName("version_id").IsRequired();
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("double precision").IsRequired();

            // Relationship with Cart
            entity.HasOne(e => e.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for faster lookups
            entity.HasIndex(e => e.CartId);
            entity.HasIndex(e => e.VersionId);
        });

        // ========================================
        // Cấu hình bảng Product_Version_Cache
        // ========================================
        modelBuilder.Entity<ProductVersionCache>(entity =>
        {
            entity.ToTable("product_version_cache");
            entity.HasKey(e => e.VersionId);
            
            entity.Property(e => e.VersionId).HasColumnName("version_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            
            // Product Master Info
            entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.ProductDescription).HasColumnName("product_description").HasMaxLength(2000);
            entity.Property(e => e.ProductStatus).HasColumnName("product_status").HasMaxLength(50);
            
            // Version Info
            entity.Property(e => e.SellerSku).HasColumnName("seller_sku").HasMaxLength(255).IsRequired();
            entity.Property(e => e.VersionName).HasColumnName("version_name").HasMaxLength(255);
            
            // Pricing
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            
            // Stock
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            
            // Shipping Dimensions
            entity.Property(e => e.WeightGrams).HasColumnName("weight_grams");
            entity.Property(e => e.LengthCm).HasColumnName("length_cm").HasColumnType("decimal(10,2)");
            entity.Property(e => e.WidthCm).HasColumnName("width_cm").HasColumnType("decimal(10,2)");
            entity.Property(e => e.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(10,2)");
            
            // Status
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            
            // Soft Delete
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").IsRequired().HasDefaultValue(false);
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            
            // Sync tracking
            entity.Property(e => e.LastUpdated).HasColumnName("last_updated").IsRequired();

            // Indexes for faster lookups
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.SellerSku);
            entity.HasIndex(e => e.IsActive);
        });

        // ========================================
        // Cấu hình bảng Orders
        // ========================================
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.OrderId);
            
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.ShopId).HasColumnName("shop_id").IsRequired();
            entity.Property(e => e.SubtotalCents).HasColumnName("subtotal_cents").HasColumnType("double precision").IsRequired();
            entity.Property(e => e.TotalAmountCents).HasColumnName("total_amount_cents").HasColumnType("double precision").IsRequired();
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.Payment).HasColumnName("payment");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(OrderStatus.PENDING);
            entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Indexes for faster lookups
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.Status);
        });

        // ========================================
        // Cấu hình bảng Order_Items
        // ========================================
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.OrderItemId);
            
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.VersionId).HasColumnName("version_id").IsRequired();
            entity.Property(e => e.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
            entity.Property(e => e.DiscountCents).HasColumnName("discount_cents").HasDefaultValue(0);
            entity.Property(e => e.TaxCents).HasColumnName("tax_cents").HasColumnType("double precision").HasDefaultValue(0);
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.LineTotalCents).HasColumnName("line_total_cents").HasColumnType("double precision").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(FulfillmentStatus.UNFULFILLED);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

            // Relationship with Order
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for faster lookups
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.VersionId);
        });
    }
}
