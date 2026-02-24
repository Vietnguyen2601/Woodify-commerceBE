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
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

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
            entity.Property(e => e.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();
            entity.Property(e => e.CompareAtPriceCents).HasColumnName("compare_at_price_cents");
            entity.Property(e => e.IsSelected).HasColumnName("is_selected");
            entity.Property(e => e.CustomizationNote).HasColumnName("customization_note");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.AddedAt).HasColumnName("added_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

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
            entity.Property(e => e.VersionNumber).HasColumnName("version_number").IsRequired();
            entity.Property(e => e.VersionName).HasColumnName("version_name").HasMaxLength(255);
            
            // Pricing
            entity.Property(e => e.PriceCents).HasColumnName("price_cents").IsRequired();
            entity.Property(e => e.BasePriceCents).HasColumnName("base_price_cents");
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10);
            
            // Stock
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            entity.Property(e => e.LowStockThreshold).HasColumnName("low_stock_threshold").HasDefaultValue(5);
            entity.Property(e => e.AllowBackorder).HasColumnName("allow_backorder").HasDefaultValue(false);
            
            // Shipping Dimensions
            entity.Property(e => e.WeightGrams).HasColumnName("weight_grams");
            entity.Property(e => e.LengthCm).HasColumnName("length_cm").HasColumnType("decimal(10,2)");
            entity.Property(e => e.WidthCm).HasColumnName("width_cm").HasColumnType("decimal(10,2)");
            entity.Property(e => e.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(10,2)");
            entity.Property(e => e.VolumeCm3).HasColumnName("volume_cm3");
            
            // Shipping Properties
            entity.Property(e => e.BulkyType).HasColumnName("bulky_type").HasMaxLength(50);
            entity.Property(e => e.IsFragile).HasColumnName("is_fragile").HasDefaultValue(false);
            entity.Property(e => e.RequiresSpecialHandling).HasColumnName("requires_special_handling").HasDefaultValue(false);
            
            // Warranty
            entity.Property(e => e.WarrantyMonths).HasColumnName("warranty_months").HasDefaultValue(12);
            entity.Property(e => e.WarrantyTerms).HasColumnName("warranty_terms");
            
            // Bundle
            entity.Property(e => e.IsBundle).HasColumnName("is_bundle").HasDefaultValue(false);
            entity.Property(e => e.BundleDiscountCents).HasColumnName("bundle_discount_cents").HasDefaultValue(0);
            
            // Images
            entity.Property(e => e.PrimaryImageUrl).HasColumnName("primary_image_url").HasMaxLength(1000);
            
            // Status
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            
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
            entity.Property(e => e.OrderCode).HasColumnName("order_code").HasMaxLength(100).IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CustomerName).HasColumnName("customer_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerEmail).HasColumnName("customer_email").HasMaxLength(255);
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.ShopName).HasColumnName("shop_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("VND");
            entity.Property(e => e.SubtotalCents).HasColumnName("subtotal_cents").IsRequired();
            entity.Property(e => e.ShippingFeeCents).HasColumnName("shipping_fee_cents").HasDefaultValue(0);
            entity.Property(e => e.DiscountCents).HasColumnName("discount_cents").HasDefaultValue(0);
            entity.Property(e => e.TaxCents).HasColumnName("tax_cents").HasDefaultValue(0);
            entity.Property(e => e.TotalAmountCents).HasColumnName("total_amount_cents").IsRequired();
            entity.Property(e => e.VoucherApplied).HasColumnName("voucher_applied");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(PaymentStatus.PENDING);
            entity.Property(e => e.PaymentTransactionId).HasColumnName("payment_transaction_id");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(OrderStatus.PENDING);
            entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");
            entity.Property(e => e.CustomerNote).HasColumnName("customer_note");
            entity.Property(e => e.ShopNote).HasColumnName("shop_note");
            entity.Property(e => e.PlacedAt).HasColumnName("placed_at").IsRequired();
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.ShippedAt).HasColumnName("shipped_at");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Unique constraint on order_code
            entity.HasIndex(e => e.OrderCode).IsUnique();
            
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
            entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(500).IsRequired();
            entity.Property(e => e.VariantName).HasColumnName("variant_name").HasMaxLength(500);
            entity.Property(e => e.SellerSku).HasColumnName("seller_sku").HasMaxLength(255).IsRequired();
            entity.Property(e => e.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();
            entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
            entity.Property(e => e.DiscountCents).HasColumnName("discount_cents").HasDefaultValue(0);
            entity.Property(e => e.TaxCents).HasColumnName("tax_cents").HasDefaultValue(0);
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.LineTotalCents).HasColumnName("line_total_cents").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(FulfillmentStatus.UNFULFILLED);
            entity.Property(e => e.ReturnedQuantity).HasColumnName("returned_quantity").HasDefaultValue(0);
            entity.Property(e => e.RefundedAmountCents).HasColumnName("refunded_amount_cents").HasDefaultValue(0);
            entity.Property(e => e.ShippingInfo).HasColumnName("shipping_info");
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
