using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShipmentService.Domain.Entities;
using System;
using System.IO;

namespace ShipmentService.Infrastructure.Data.Context;

public class ShipmentDbContext : DbContext
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options) : base(options)
    {
    }

    // Constructor mặc định cho design-time (migrations)
    public ShipmentDbContext() : base()
    {
    }

    // ── DbSets ────────────────────────────────────────────────────────────────
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<ShippingProvider> ShippingProviders { get; set; } = null!;
    public DbSet<ProviderService> ProviderServices { get; set; } = null!;

    // ── Model configuration ───────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Shipments ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments");
            entity.HasKey(e => e.ShipmentId);
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");

            entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(e => e.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(50);
            entity.Property(e => e.ProviderServiceId).HasColumnName("provider_service_id");

            // Địa chỉ lưu dưới dạng nvarchar
            entity.Property(e => e.PickupAddressId).HasColumnName("pickup_address_id");
            entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");

            entity.Property(e => e.TotalWeightGrams).HasColumnName("total_weight_grams").IsRequired();
            entity.Property(e => e.BulkyType).HasColumnName("bulky_type");
            entity.Property(e => e.FinalShippingFeeCents).HasColumnName("final_shipping_fee_cents").IsRequired();
            entity.Property(e => e.IsFreeShipping).HasColumnName("is_free_shipping").HasDefaultValue(false);

            entity.Property(e => e.PickupScheduledAt).HasColumnName("pickup_scheduled_at");
            entity.Property(e => e.PickedUpAt).HasColumnName("picked_up_at");
            entity.Property(e => e.DeliveryEstimatedAt).HasColumnName("delivery_estimated_at");

            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("PENDING").IsRequired();
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // FK -> ProviderServices
            entity.HasOne(e => e.ProviderService)
                  .WithMany(ps => ps.Shipments)
                  .HasForeignKey(e => e.ProviderServiceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── ShippingProviders ──────────────────────────────────────────────────
        modelBuilder.Entity<ShippingProvider>(entity =>
        {
            entity.ToTable("Shipping_Providers");
            entity.HasKey(e => e.ProviderId);
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");

            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.SupportPhone).HasColumnName("support_phone");
            entity.Property(e => e.SupportEmail).HasColumnName("support_email");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // ── ProviderServices ───────────────────────────────────────────────────
        modelBuilder.Entity<ProviderService>(entity =>
        {
            entity.ToTable("Provider_Services");
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.Property(e => e.ProviderId).HasColumnName("provider_id").IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.SpeedLevel).HasColumnName("speed_level");
            entity.Property(e => e.EstimatedDaysMin).HasColumnName("estimated_days_min");
            entity.Property(e => e.EstimatedDaysMax).HasColumnName("estimated_days_max");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.MultiplierFee).HasColumnName("multiplier_fee");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // FK -> ShippingProviders
            entity.HasOne(e => e.ShippingProvider)
                  .WithMany(sp => sp.ProviderServices)
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ── Design-time connection (for dotnet ef migrations) ─────────────────────
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = GetConnectionString("ShipmentService");
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
        // Tìm từ thư mục hiện tại (hoạt động đúng khi chạy dotnet ef migrations)
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, ".env")))
                return Path.Combine(dir.FullName, ".env");
            dir = dir.Parent;
        }
        return null;
    }
}
