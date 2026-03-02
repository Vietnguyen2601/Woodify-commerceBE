using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Enums;
using System;
using System.IO;

namespace ShipmentService.Infrastructure.Data.Context;

public class ShipmentDbContext : DbContext
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options) : base(options)
    {
    }

    // ─── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Shipment> Shipments { get; set; } = null!;
    public DbSet<ProviderService> ProviderServices { get; set; } = null!;

    // ─── Model Configuration ──────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── ProviderService ───────────────────────────────────────────────────
        modelBuilder.Entity<ProviderService>(entity =>
        {
            entity.ToTable("provider_services");
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ProviderCode).HasColumnName("provider_code").IsRequired();
            entity.Property(e => e.ProviderName).HasColumnName("provider_name").IsRequired();
            entity.Property(e => e.ProviderLogoUrl).HasColumnName("provider_logo_url");
            entity.Property(e => e.ServiceCode).HasColumnName("service_code");
            entity.Property(e => e.ServiceName).HasColumnName("service_name").IsRequired();
            entity.Property(e => e.SpeedLevel)
                  .HasColumnName("speed_level")
                  .HasConversion<string>();
            entity.Property(e => e.EstimatedDeliveryDays).HasColumnName("estimated_delivery_days");
            entity.Property(e => e.Limitations).HasColumnName("limitations");
            entity.Property(e => e.ZoneConfig).HasColumnName("zone_config");
            entity.Property(e => e.PricingRules).HasColumnName("pricing_rules");
            entity.Property(e => e.PlatformFeeConfig).HasColumnName("platform_fee_config");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.PriorityOrder).HasColumnName("priority_order").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // ── Shipment ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("shipments");
            entity.HasKey(e => e.ShipmentId);
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ShipmentCode).HasColumnName("shipment_code").IsRequired();
            entity.Property(e => e.ProviderServiceId).HasColumnName("provider_service_id");
            entity.Property(e => e.TrackingNumber).HasColumnName("tracking_number");
            entity.Property(e => e.PickupAddressId).HasColumnName("pickup_address_id");
            entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");
            entity.Property(e => e.TotalWeightGrams).HasColumnName("total_weight_grams");
            entity.Property(e => e.TotalVolumeCm3).HasColumnName("total_volume_cm3");
            entity.Property(e => e.PackageCount).HasColumnName("package_count").HasDefaultValue(1);
            entity.Property(e => e.BulkyType)
                  .HasColumnName("bulky_type")
                  .HasConversion<string>()
                  .HasDefaultValue(BulkyType.Normal);
            entity.Property(e => e.IsFragile).HasColumnName("is_fragile").HasDefaultValue(false);
            entity.Property(e => e.RequiresInsurance).HasColumnName("requires_insurance").HasDefaultValue(false);
            entity.Property(e => e.InsuranceFeeCents).HasColumnName("insurance_fee_cents").HasDefaultValue(0.0);
            entity.Property(e => e.FinalShippingFeeCents).HasColumnName("final_shipping_fee_cents");
            entity.Property(e => e.IsFreeShipping).HasColumnName("is_free_shipping").HasDefaultValue(false);
            entity.Property(e => e.PickupScheduledAt).HasColumnName("pickup_scheduled_at");
            entity.Property(e => e.PickedUpAt).HasColumnName("picked_up_at");
            entity.Property(e => e.DeliveryEstimatedAt).HasColumnName("delivery_estimated_at");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.ReturnedAt).HasColumnName("returned_at");
            entity.Property(e => e.Status)
                  .HasColumnName("status")
                  .HasConversion<string>()
                  .HasDefaultValue(ShipmentStatus.Pending);
            entity.Property(e => e.FailureReason).HasColumnName("failure_reason");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CustomerNote).HasColumnName("customer_note");
            entity.Property(e => e.InternalNote).HasColumnName("internal_note");
            entity.Property(e => e.DeliveryInstruction).HasColumnName("delivery_instruction");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ConfirmedBy).HasColumnName("confirmed_by");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            // Unique indexes
            entity.HasIndex(e => e.ShipmentCode).IsUnique();
            entity.HasIndex(e => e.TrackingNumber).IsUnique();

            // FK -> ProviderService
            entity.HasOne(e => e.ProviderService)
                  .WithMany(ps => ps.Shipments)
                  .HasForeignKey(e => e.ProviderServiceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

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
}
