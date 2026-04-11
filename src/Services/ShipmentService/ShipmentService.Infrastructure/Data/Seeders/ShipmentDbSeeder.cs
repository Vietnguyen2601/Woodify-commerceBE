using Microsoft.EntityFrameworkCore;
using ShipmentService.Domain.Entities;

namespace ShipmentService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder cho ShipmentService Database
/// Khởi tạo dữ liệu ShippingProviders, ProviderServices và Shipments mặc định
/// </summary>
public static class ShipmentDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as ShipmentService.Infrastructure.Data.Context.ShipmentDbContext;
        if (dbContext == null)
            return;

        // ── 1. Seed ShippingProviders ─────────────────────────────────────────
        if (!dbContext.ShippingProviders.Any())
        {
            var ghnId = Guid.NewGuid();
            var ghtkId = Guid.NewGuid();
            var viettelId = Guid.NewGuid();
            var jtId = Guid.NewGuid();
            var vnpostId = Guid.NewGuid();

            var providers = new List<ShippingProvider>
            {
                new()
                {
                    ProviderId   = ghnId,
                    Name         = "Giao Hàng Nhanh (GHN)",
                    SupportPhone = "1900 636 677",
                    SupportEmail = "cskh@ghn.vn",
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                },
                new()
                {
                    ProviderId   = ghtkId,
                    Name         = "Giao Hàng Tiết Kiệm (GHTK)",
                    SupportPhone = "1900 636 507",
                    SupportEmail = "hotro@ghtk.vn",
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                },
                new()
                {
                    ProviderId   = viettelId,
                    Name         = "Viettel Post",
                    SupportPhone = "1800 8055",
                    SupportEmail = "cskh@viettelpost.vn",
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                },
                new()
                {
                    ProviderId   = jtId,
                    Name         = "J&T Express",
                    SupportPhone = "1900 633 878",
                    SupportEmail = "support@jtexpress.vn",
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                },
                new()
                {
                    ProviderId   = vnpostId,
                    Name         = "Vietnam Post (VNPost)",
                    SupportPhone = "1800 1199",
                    SupportEmail = "cskh@vnpost.vn",
                    IsActive     = true,
                    CreatedAt    = DateTime.UtcNow,
                    UpdatedAt    = DateTime.UtcNow
                }
            };

            dbContext.ShippingProviders.AddRange(providers);
            await dbContext.SaveChangesAsync();

            // ── 2. Seed ProviderServices ──────────────────────────────────────
            var ghnEcoId = Guid.NewGuid();
            var ghnStdId = Guid.NewGuid();
            var ghnExpId = Guid.NewGuid();
            var ghtkEcoId = Guid.NewGuid();
            var ghtkStdId = Guid.NewGuid();
            var vtpStdId = Guid.NewGuid();
            var vtpExpId = Guid.NewGuid();
            var jtStdId = Guid.NewGuid();
            var jtExpId = Guid.NewGuid();
            var vnpStdId = Guid.NewGuid();

            var services = new List<ProviderService>
            {
                // GHN
                new()
                {
                    ServiceId        = ghnEcoId,
                    ProviderId       = ghnId,
                    Code             = "ECO",
                    Name             = "GHN Tiết Kiệm",
                    SpeedLevel       = "ECONOMY",
                    EstimatedDaysMin = 3,
                    EstimatedDaysMax = 5,
                    MultiplierFee    = 1.0,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                new()
                {
                    ServiceId        = ghnStdId,
                    ProviderId       = ghnId,
                    Code             = "STD",
                    Name             = "GHN Nhanh",
                    SpeedLevel       = "STANDARD",
                    EstimatedDaysMin = 1,
                    EstimatedDaysMax = 3,
                    MultiplierFee    = 1.3,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                new()
                {
                    ServiceId        = ghnExpId,
                    ProviderId       = ghnId,
                    Code             = "EXP",
                    Name             = "GHN Hỏa Tốc",
                    SpeedLevel       = "EXPRESS",
                    EstimatedDaysMin = 0,
                    EstimatedDaysMax = 1,
                    MultiplierFee    = 1.8,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                // GHTK
                new()
                {
                    ServiceId        = ghtkEcoId,
                    ProviderId       = ghtkId,
                    Code             = "ECO",
                    Name             = "GHTK Tiết Kiệm",
                    SpeedLevel       = "ECONOMY",
                    EstimatedDaysMin = 4,
                    EstimatedDaysMax = 6,
                    MultiplierFee    = 0.9,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                new()
                {
                    ServiceId        = ghtkStdId,
                    ProviderId       = ghtkId,
                    Code             = "STD",
                    Name             = "GHTK Nhanh",
                    SpeedLevel       = "STANDARD",
                    EstimatedDaysMin = 2,
                    EstimatedDaysMax = 3,
                    MultiplierFee    = 1.2,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                // Viettel Post
                new()
                {
                    ServiceId        = vtpStdId,
                    ProviderId       = viettelId,
                    Code             = "STD",
                    Name             = "VTP Nhanh",
                    SpeedLevel       = "STANDARD",
                    EstimatedDaysMin = 2,
                    EstimatedDaysMax = 4,
                    MultiplierFee    = 1.1,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                new()
                {
                    ServiceId        = vtpExpId,
                    ProviderId       = viettelId,
                    Code             = "EXP",
                    Name             = "VTP Hỏa Tốc",
                    SpeedLevel       = "EXPRESS",
                    EstimatedDaysMin = 1,
                    EstimatedDaysMax = 2,
                    MultiplierFee    = 1.6,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                // J&T Express
                new()
                {
                    ServiceId        = jtStdId,
                    ProviderId       = jtId,
                    Code             = "STD",
                    Name             = "J&T Nhanh",
                    SpeedLevel       = "STANDARD",
                    EstimatedDaysMin = 2,
                    EstimatedDaysMax = 3,
                    MultiplierFee    = 1.15,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                new()
                {
                    ServiceId        = jtExpId,
                    ProviderId       = jtId,
                    Code             = "EXP",
                    Name             = "J&T Hỏa Tốc",
                    SpeedLevel       = "EXPRESS",
                    EstimatedDaysMin = 1,
                    EstimatedDaysMax = 1,
                    MultiplierFee    = 1.7,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                },
                // VNPost
                new()
                {
                    ServiceId        = vnpStdId,
                    ProviderId       = vnpostId,
                    Code             = "STD",
                    Name             = "VNPost Bưu Kiện",
                    SpeedLevel       = "STANDARD",
                    EstimatedDaysMin = 3,
                    EstimatedDaysMax = 7,
                    MultiplierFee    = 0.85,
                    IsActive         = true,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                }
            };

            dbContext.ProviderServices.AddRange(services);
            await dbContext.SaveChangesAsync();

            // ── 3. Seed Shipments ─────────────────────────────────────────────
            if (!dbContext.Shipments.Any())
            {
                var now = DateTime.UtcNow;

                var shipments = new List<Shipment>
                {
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "GHN2026030001",
                        ProviderServiceId      = ghnStdId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "456 Lê Lợi, Q.10, TP.HCM",
                        TotalWeightGrams       = 1200,
                        BulkyType              = "NORMAL",
                        FinalShippingFeeVnd  = 3500000,
                        IsFreeShipping         = false,
                        PickupScheduledAt      = now.AddHours(2),
                        PickedUpAt             = now.AddHours(3),
                        DeliveryEstimatedAt    = now.AddDays(2),
                        Status                 = "IN_TRANSIT",
                        CreatedAt              = now.AddDays(-1),
                        UpdatedAt              = now
                    },
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "GHN2026030002",
                        ProviderServiceId      = ghnExpId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "789 Trần Hưng Đạo, Q.5, TP.HCM",
                        TotalWeightGrams       = 800,
                        BulkyType              = "NORMAL",
                        FinalShippingFeeVnd  = 6000000,
                        IsFreeShipping         = false,
                        PickupScheduledAt      = now.AddHours(1),
                        PickedUpAt             = now.AddHours(1.5),
                        DeliveryEstimatedAt    = now.AddHours(8),
                        Status                 = "OUT_FOR_DELIVERY",
                        CreatedAt              = now.AddHours(-5),
                        UpdatedAt              = now
                    },
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "GHTK2026030001",
                        ProviderServiceId      = ghtkEcoId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "321 Cách Mạng Tháng Tám, Q.3, TP.HCM",
                        TotalWeightGrams       = 2500,
                        BulkyType              = "NORMAL",
                        FinalShippingFeeVnd  = 2500000,
                        IsFreeShipping         = true,
                        PickupScheduledAt      = now.AddDays(1),
                        DeliveryEstimatedAt    = now.AddDays(5),
                        Status                 = "PENDING",
                        CreatedAt              = now,
                        UpdatedAt              = now
                    },
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "VTP2026030001",
                        ProviderServiceId      = vtpStdId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "654 Võ Văn Tần, Q.3, TP.HCM",
                        TotalWeightGrams       = 5000,
                        BulkyType              = "BULKY",
                        FinalShippingFeeVnd  = 8500000,
                        IsFreeShipping         = false,
                        PickupScheduledAt      = now.AddDays(-3),
                        PickedUpAt             = now.AddDays(-3).AddHours(1),
                        DeliveryEstimatedAt    = now.AddDays(-1),
                        Status                 = "DELIVERED",
                        CreatedAt              = now.AddDays(-4),
                        UpdatedAt              = now.AddDays(-1)
                    },
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "JT2026030001",
                        ProviderServiceId      = jtStdId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "97 Đinh Tiên Hoàng, Q.Bình Thạnh, TP.HCM",
                        TotalWeightGrams       = 1800,
                        BulkyType              = "NORMAL",
                        FinalShippingFeeVnd  = 4200000,
                        IsFreeShipping         = false,
                        PickupScheduledAt      = now.AddDays(-2),
                        PickedUpAt             = now.AddDays(-2).AddHours(2),
                        DeliveryEstimatedAt    = now.AddDays(-1),
                        Status                 = "DELIVERY_FAILED",
                        FailureReason          = "Khách hàng không có mặt tại địa chỉ giao hàng",
                        CreatedAt              = now.AddDays(-3),
                        UpdatedAt              = now.AddDays(-1)
                    },
                    new()
                    {
                        ShipmentId             = Guid.NewGuid(),
                        OrderId                = Guid.NewGuid(),
                        TrackingNumber         = "GHN2026030003",
                        ProviderServiceId      = ghnEcoId,
                        PickupAddressId        = "Kho Woodify - 123 Nguyễn Huệ, Q.1, TP.HCM",
                        DeliveryAddressId      = "200 Hoàng Văn Thụ, Q.Phú Nhuận, TP.HCM",
                        TotalWeightGrams       = 600,
                        BulkyType              = "NORMAL",
                        FinalShippingFeeVnd  = 2000000,
                        IsFreeShipping         = false,
                        Status                 = "CANCELLED",
                        CancelReason           = "Khách hàng hủy đơn hàng trước khi lấy hàng",
                        CreatedAt              = now.AddDays(-5),
                        UpdatedAt              = now.AddDays(-5)
                    }
                };

                dbContext.Shipments.AddRange(shipments);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
