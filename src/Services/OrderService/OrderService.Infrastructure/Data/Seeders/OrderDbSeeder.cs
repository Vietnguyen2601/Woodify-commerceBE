using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Data.Seeders;

/// <summary>
/// Seed data cho OrderService.
///
/// Flow: Account → Shop → Product → Cart → Order → Payment
///
/// Math (mỗi order):
///   SubtotalCents    = Σ(UnitPriceCents × Qty)
///   CommissionCents  = ⌊SubtotalCents × 0.06⌋
///   ProductDiscount  = MIN(subtotal × voucherValue/100, maxDiscount)   [PRODUCT_PERCENTAGE]
///   ShipDiscount     = shippingFee                                      [SHIPPING_FREE]
///   TotalAmountCents = Subtotal − ProductDiscount + ShipFee − ShipDiscount
/// </summary>
public static class OrderDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var db = context as OrderService.Infrastructure.Data.Context.OrderDbContext;
        if (db == null) return;

        // ══════════════════════════════════════════════════════════════════
        // CROSS-SERVICE REFERENCE IDs  (must stay in sync with other seeders)
        // ══════════════════════════════════════════════════════════════════

        // Accounts (IdentityService seeder)
        var cust01 = new Guid("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"); // Lê Văn Nam
        var cust02 = new Guid("d2e3f4a5-b6c7-4d8e-9f0a-1b2c3d4e5f6a"); // Phạm Thị Mai
        var cust03 = new Guid("e3f4a5b6-c7d8-4e9f-0a1b-2c3d4e5f6a7b"); // Nguyễn Thị Hoa
        var seller01 = new Guid("a7b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d"); // Owner – Nội Thất Gỗ A
        var seller02 = new Guid("b8c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e"); // Owner – Gỗ Hiện Đại B

        // Shops (ShopService seeder)
        var shop01 = new Guid("f4a5b6c7-d8e9-4f0a-1b2c-3d4e5f6a7b8c"); // Nội Thất Gỗ Cao Cấp A
        var shop02 = new Guid("a5b6c7d8-e9f0-4a1b-2c3d-4e5f6a7b8c9d"); // Nội Thất Gỗ Hiện Đại B

        // Product version IDs (ProductService seeder) – unit price in "cents"
        var sofaV1 = new Guid("f0a1b2c3-d4e5-4f6a-7b8c-9d0e1f2a3b4c"); // Shop01 – 25 000
        var sofaV2 = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"); // Shop01 – 32 000
        var bedV1 = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"); // Shop01 – 18 000
        var bedV2 = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"); // Shop01 – 22 000
        var tableV1 = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"); // Shop02 – 32 000
        var tableV2 = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"); // Shop02 – 38 000
        var deskV1 = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"); // Shop02 –  8 500
        var deskV2 = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"); // Shop02 – 12 000

        // Shipping fees
        const long SHIP_STD = 300;  // Giao hàng tiêu chuẩn
        const long SHIP_EXP = 500;  // Giao hàng nhanh

        // ══════════════════════════════════════════════════════════════════
        // 1. PRODUCT VERSION CACHES
        // ══════════════════════════════════════════════════════════════════
        if (!db.ProductVersionCaches.Any())
        {
            db.ProductVersionCaches.AddRange(
                // ── Shop01 – Sofa
                new ProductVersionCache { VersionId = sofaV1, ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"), ShopId = shop01, ProductName = "Ghế Sofa Gỗ Óc Chó Cao Cấp", ProductDescription = "Ghế sofa phiên bản tiêu chuẩn từ gỗ óc chó nguyên khối", ProductStatus = "PUBLISHED", SellerSku = "SOFA-OC-CHO-V1", VersionNumber = 1, VersionName = "Phiên bản tiêu chuẩn", Price = 25000.00, Currency = "VND", StockQuantity = 10, WoodType = "Óc Chó", WeightGrams = 85000, LengthCm = 200, WidthCm = 90, HeightCm = 80, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                new ProductVersionCache { VersionId = sofaV2, ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"), ShopId = shop01, ProductName = "Ghế Sofa Gỗ Óc Chó Cao Cấp", ProductDescription = "Ghế sofa phiên bản sang trọng – da thật + gỗ óc chó", ProductStatus = "PUBLISHED", SellerSku = "SOFA-OC-CHO-V2", VersionNumber = 2, VersionName = "Phiên bản sang trọng", Price = 32000.00, Currency = "VND", StockQuantity = 5, WoodType = "Óc Chó", WeightGrams = 90000, LengthCm = 200, WidthCm = 100, HeightCm = 85, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                // ── Shop01 – Bed
                new ProductVersionCache { VersionId = bedV1, ProductId = new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"), ShopId = shop01, ProductName = "Giường Ngủ Gỗ Sồi 1m8", ProductDescription = "Giường ngủ gỗ sồi tự nhiên có ngăn kéo tiện dụng", ProductStatus = "PUBLISHED", SellerSku = "BED-SOI-1M8-V1", VersionNumber = 1, VersionName = "Giường có ngăn kéo", Price = 18000.00, Currency = "VND", StockQuantity = 15, WoodType = "Sồi", WeightGrams = 120000, LengthCm = 200, WidthCm = 180, HeightCm = 40, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                new ProductVersionCache { VersionId = bedV2, ProductId = new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"), ShopId = shop01, ProductName = "Giường Ngủ Gỗ Sồi 1m8", ProductDescription = "Giường ngủ cao cấp gỗ sồi ngoại nhập – đầu giường bọc da", ProductStatus = "PUBLISHED", SellerSku = "BED-SOI-1M8-V2", VersionNumber = 2, VersionName = "Giường cao cấp ngoại nhập", Price = 22000.00, Currency = "VND", StockQuantity = 8, WoodType = "Sồi", WeightGrams = 125000, LengthCm = 200, WidthCm = 180, HeightCm = 45, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                // ── Shop02 – Dining Table
                new ProductVersionCache { VersionId = tableV1, ProductId = new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"), ShopId = shop02, ProductName = "Bàn Ăn Gỗ Teak 6 Ghế", ProductDescription = "Combo bàn ăn 6 ghế gỗ teak nguyên khối cao cấp", ProductStatus = "PUBLISHED", SellerSku = "TABLE-TEAK-6G-V1", VersionNumber = 1, VersionName = "Combo bàn + 6 ghế", Price = 32000.00, Currency = "VND", StockQuantity = 8, WoodType = "Teak", WeightGrams = 95000, LengthCm = 160, WidthCm = 90, HeightCm = 75, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                new ProductVersionCache { VersionId = tableV2, ProductId = new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"), ShopId = shop02, ProductName = "Bàn Ăn Gỗ Teak 6 Ghế", ProductDescription = "Combo bàn ăn mở rộng 6+2 ghế gỗ teak nhập khẩu", ProductStatus = "PUBLISHED", SellerSku = "TABLE-TEAK-6G-V2", VersionNumber = 2, VersionName = "Combo bàn ăn mở rộng", Price = 38000.00, Currency = "VND", StockQuantity = 5, WoodType = "Teak", WeightGrams = 100000, LengthCm = 180, WidthCm = 100, HeightCm = 75, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                // ── Shop02 – Desk
                new ProductVersionCache { VersionId = deskV1, ProductId = new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"), ShopId = shop02, ProductName = "Bàn Làm Việc Gỗ Ash Hiện Đại", ProductDescription = "Bàn làm việc gỗ ash phong cách Bắc Âu – ngăn kéo + giá sách", ProductStatus = "PUBLISHED", SellerSku = "DESK-ASH-MOD-V1", VersionNumber = 1, VersionName = "Bàn có ngăn kéo và giá sách", Price = 8500.00, Currency = "VND", StockQuantity = 20, WoodType = "Ash", WeightGrams = 35000, LengthCm = 140, WidthCm = 60, HeightCm = 75, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow },
                new ProductVersionCache { VersionId = deskV2, ProductId = new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"), ShopId = shop02, ProductName = "Bàn Làm Việc Gỗ Ash Hiện Đại", ProductDescription = "Bàn làm việc Premium gỗ ash – mặt bàn dày 40mm, chân thép", ProductStatus = "PUBLISHED", SellerSku = "DESK-ASH-MOD-V2", VersionNumber = 2, VersionName = "Bàn làm việc Premium", Price = 12000.00, Currency = "VND", StockQuantity = 12, WoodType = "Ash", WeightGrams = 38000, LengthCm = 160, WidthCm = 70, HeightCm = 75, IsActive = true, IsDeleted = false, LastUpdated = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════
        // 2. VOUCHERS  (6 vouchers)
        //   WELCOME10  – PRODUCT_PERCENTAGE 10%, max=2000, platform, stackable
        //   SHIP_FREE  – SHIPPING_FREE, platform, stackable
        //   SHOP1_15   – PRODUCT_PERCENTAGE 15%, max=3600, shop01 only, stackable
        //   SHOP2_20   – PRODUCT_PERCENTAGE 20%, max=6000, shop02 only, stackable
        //   NOSPLIT    – PRODUCT_PERCENTAGE 5%, NOT stackable
        //   EXPIRED30  – expired voucher (for testing validation)
        // ══════════════════════════════════════════════════════════════════
        var vWelcome10 = new Guid("cc000001-0000-4000-8000-000000000001");
        var vShipFree = new Guid("cc000002-0000-4000-8000-000000000002");
        var vShop115 = new Guid("cc000003-0000-4000-8000-000000000003");
        var vShop220 = new Guid("cc000004-0000-4000-8000-000000000004");
        var vNosplit = new Guid("cc000005-0000-4000-8000-000000000005");
        var vExpired = new Guid("cc000006-0000-4000-8000-000000000006");

        if (!db.Vouchers.Any())
        {
            db.Vouchers.AddRange(
                new Voucher
                {
                    VoucherId = vWelcome10,
                    Code = "WELCOME10",
                    Name = "Chào mừng khách hàng mới – giảm 10%",
                    Description = "Giảm 10% đơn hàng, tối đa 2.000đ. Áp dụng toàn sàn.",
                    Type = VoucherType.PRODUCT_PERCENTAGE,
                    DiscountValue = 10,
                    MinOrderAmountCents = 0,
                    MaxDiscountCents = 2000,
                    UsageLimit = 1000,
                    UsageCount = 5,
                    MaxUsagePerUser = 3,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    ApplicableShopId = null,
                    IsActive = true,
                    IsStackable = true,
                    CreatedBy = seller01,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Voucher
                {
                    VoucherId = vShipFree,
                    Code = "SHIP_FREE",
                    Name = "Miễn phí vận chuyển",
                    Description = "Miễn toàn bộ phí vận chuyển. Áp dụng toàn sàn.",
                    Type = VoucherType.SHIPPING_FREE,
                    DiscountValue = 100,
                    MinOrderAmountCents = 0,
                    MaxDiscountCents = 0,
                    UsageLimit = 500,
                    UsageCount = 5,
                    MaxUsagePerUser = 2,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    ApplicableShopId = null,
                    IsActive = true,
                    IsStackable = true,
                    CreatedBy = seller01,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Voucher
                {
                    VoucherId = vShop115,
                    Code = "SHOP1_15",
                    Name = "Nội Thất Gỗ A – Giảm 15%",
                    Description = "Giảm 15% cho đơn từ 15.000đ tại Shop01. Tối đa 3.600đ.",
                    Type = VoucherType.PRODUCT_PERCENTAGE,
                    DiscountValue = 15,
                    MinOrderAmountCents = 15000,
                    MaxDiscountCents = 3600,
                    UsageLimit = 200,
                    UsageCount = 3,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    ApplicableShopId = shop01,
                    IsActive = true,
                    IsStackable = true,
                    CreatedBy = seller01,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Voucher
                {
                    VoucherId = vShop220,
                    Code = "SHOP2_20",
                    Name = "Nội Thất Gỗ B – Giảm 20%",
                    Description = "Giảm 20% cho đơn từ 20.000đ tại Shop02. Tối đa 6.000đ.",
                    Type = VoucherType.PRODUCT_PERCENTAGE,
                    DiscountValue = 20,
                    MinOrderAmountCents = 20000,
                    MaxDiscountCents = 6000,
                    UsageLimit = 200,
                    UsageCount = 2,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    ApplicableShopId = shop02,
                    IsActive = true,
                    IsStackable = true,
                    CreatedBy = seller02,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Voucher
                {
                    VoucherId = vNosplit,
                    Code = "NOSPLIT",
                    Name = "Ưu đãi độc quyền – Không được ghép",
                    Description = "Giảm 5% toàn sàn, đơn từ 50.000đ. KHÔNG thể dùng cùng voucher khác.",
                    Type = VoucherType.PRODUCT_PERCENTAGE,
                    DiscountValue = 5,
                    MinOrderAmountCents = 50000,
                    MaxDiscountCents = 2000,
                    UsageLimit = 100,
                    UsageCount = 0,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(365),
                    ApplicableShopId = null,
                    IsActive = true,
                    IsStackable = false,
                    CreatedBy = seller01,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Voucher
                {
                    VoucherId = vExpired,
                    Code = "EXPIRED30",
                    Name = "Khuyến mãi đã hết hạn – 30%",
                    Description = "Voucher đã hết hạn sử dụng. Chỉ dùng để test validation.",
                    Type = VoucherType.PRODUCT_PERCENTAGE,
                    DiscountValue = 30,
                    MinOrderAmountCents = 0,
                    MaxDiscountCents = 10000,
                    UsageLimit = 50,
                    UsageCount = 0,
                    MaxUsagePerUser = 1,
                    ValidFrom = DateTime.UtcNow.AddDays(-90),
                    ValidTo = DateTime.UtcNow.AddDays(-1),
                    ApplicableShopId = null,
                    IsActive = true,
                    IsStackable = true,
                    CreatedBy = seller01,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                }
            );
            await db.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════
        // 3. CARTS  (current active cart for 3 customers – not yet checked out)
        // ══════════════════════════════════════════════════════════════════
        if (!db.Carts.Any())
        {
            db.Carts.AddRange(
                new Cart
                {
                    CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                    AccountId = cust01,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CartItems = new List<CartItem>
                    {
                        new CartItem { CartItemId = new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"), CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"), VersionId = sofaV1, ShopId = shop01, Quantity = 1, Price = 25000.00 },
                        new CartItem { CartItemId = new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"), CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"), VersionId = bedV1,  ShopId = shop01, Quantity = 1, Price = 18000.00 }
                    }
                },
                new Cart
                {
                    CartId = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"),
                    AccountId = cust02,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    CartItems = new List<CartItem>
                    {
                        new CartItem { CartItemId = new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"), CartId = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"), VersionId = tableV2, ShopId = shop02, Quantity = 1, Price = 38000.00 }
                    }
                },
                new Cart
                {
                    CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"),
                    AccountId = cust03,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CartItems = new List<CartItem>
                    {
                        new CartItem { CartItemId = new Guid("b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e"), CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"), VersionId = deskV2,  ShopId = shop02, Quantity = 1, Price = 12000.00 },
                        new CartItem { CartItemId = new Guid("c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f"), CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"), VersionId = sofaV2,  ShopId = shop01, Quantity = 1, Price = 32000.00 }
                    }
                }
            );
            await db.SaveChangesAsync();
        }

        // ══════════════════════════════════════════════════════════════════
        // 4. ORDERS + ORDER ITEMS  (20 orders: 7 PENDING, 13 completed)
        //
        //  # │ Buyer    │ Shop  │ Products                        │ Voucher(s)          │ Subtotal │ Ship │ ProdDisc │ ShipDisc │ Total  │ Comm  │ Status
        //  1 │ cust01   │ shop1 │ sofaV1×1                        │ –                   │  25 000  │  300 │     0    │    0     │ 25 300 │  1500 │ DELIVERED
        //  2 │ cust02   │ shop2 │ tableV2×1                       │ WELCOME10           │  38 000  │  300 │  2 000   │    0     │ 36 300 │  2280 │ DELIVERED
        //  3 │ cust01   │ shop1 │ bedV1×1 + sofaV1×1              │ SHIP_FREE           │  43 000  │  300 │     0    │  300     │ 43 000 │  2580 │ DELIVERED
        //  4 │ cust03   │ shop1 │ sofaV2×1                        │ WELCOME10+SHIP_FREE │  32 000  │  300 │  2 000   │  300     │ 30 000 │  1920 │ DELIVERED
        //  5 │ seller01 │ shop2 │ deskV2×2 + tableV1×1            │ SHOP2_20            │  56 000  │  500 │  6 000   │    0     │ 50 500 │  3360 │ DELIVERED
        //  6 │ seller02 │ shop1 │ bedV2×1                         │ –                   │  22 000  │  300 │     0    │    0     │ 22 300 │  1320 │ SHIPPED
        //  7 │ cust01   │ shop2 │ deskV1×3                        │ WELCOME10           │  25 500  │  300 │  2 000   │    0     │ 23 800 │  1530 │ SHIPPED
        //  8 │ cust02   │ shop1 │ sofaV1×1                        │ –                   │  25 000  │  300 │     0    │    0     │ 25 300 │  1500 │ SHIPPED
        //  9 │ cust03   │ shop2 │ tableV2×1 + deskV2×1            │ SHOP2_20+SHIP_FREE  │  50 000  │  500 │  6 000   │  500     │ 44 000 │  3000 │ CONFIRMED
        // 10 │ seller01 │ shop1 │ bedV1×2 + bedV2×1               │ SHOP1_15            │  58 000  │  300 │  3 600   │    0     │ 54 700 │  3480 │ CONFIRMED
        // 11 │ seller02 │ shop2 │ deskV1×2                        │ –                   │  17 000  │  300 │     0    │    0     │ 17 300 │  1020 │ PROCESSING
        // 12 │ cust02   │ shop1 │ sofaV2×1 + bedV1×1              │ SHOP1_15            │  50 000  │  500 │  3 600   │    0     │ 46 900 │  3000 │ CANCELLED
        // 13 │ cust03   │ shop2 │ tableV1×1                       │ –                   │  32 000  │  300 │     0    │    0     │ 32 300 │  1920 │ REFUNDED
        // 14 │ cust01   │ shop1 │ sofaV1×1 + bedV1×1              │ –                   │  43 000  │  300 │     0    │    0     │ 43 300 │  2580 │ PENDING
        // 15 │ cust02   │ shop2 │ tableV2×2                       │ WELCOME10           │  76 000  │  500 │  2 000   │    0     │ 74 500 │  4560 │ PENDING
        // 16 │ cust03   │ shop1 │ sofaV2×1                        │ SHIP_FREE           │  32 000  │  300 │     0    │  300     │ 32 000 │  1920 │ PENDING
        // 17 │ seller01 │ shop2 │ deskV2×1 + tableV1×1            │ –                   │  44 000  │  300 │     0    │    0     │ 44 300 │  2640 │ PENDING
        // 18 │ seller02 │ shop1 │ bedV2×2 + sofaV1×1              │ SHOP1_15+SHIP_FREE  │  69 000  │  500 │  3 600   │  500     │ 65 400 │  4140 │ PENDING
        // 19 │ cust01   │ shop2 │ tableV2×1                       │ –                   │  38 000  │  300 │     0    │    0     │ 38 300 │  2280 │ PENDING
        // 20 │ cust02   │ shop1 │ sofaV1×1 + sofaV2×1             │ WELCOME10           │  57 000  │  300 │  2 000   │    0     │ 55 300 │  3420 │ PENDING
        // ══════════════════════════════════════════════════════════════════
        if (!db.Orders.Any())
        {
            // ── Order IDs
            var ord01 = new Guid("dd000001-0000-4000-8000-000000000001");
            var ord02 = new Guid("dd000002-0000-4000-8000-000000000002");
            var ord03 = new Guid("dd000003-0000-4000-8000-000000000003");
            var ord04 = new Guid("dd000004-0000-4000-8000-000000000004");
            var ord05 = new Guid("dd000005-0000-4000-8000-000000000005");
            var ord06 = new Guid("dd000006-0000-4000-8000-000000000006");
            var ord07 = new Guid("dd000007-0000-4000-8000-000000000007");
            var ord08 = new Guid("dd000008-0000-4000-8000-000000000008");
            var ord09 = new Guid("dd000009-0000-4000-8000-000000000009");
            var ord10 = new Guid("dd000010-0000-4000-8000-000000000010");
            var ord11 = new Guid("dd000011-0000-4000-8000-000000000011");
            var ord12 = new Guid("dd000012-0000-4000-8000-000000000012");
            var ord13 = new Guid("dd000013-0000-4000-8000-000000000013");
            var ord14 = new Guid("dd000014-0000-4000-8000-000000000014");
            var ord15 = new Guid("dd000015-0000-4000-8000-000000000015");
            var ord16 = new Guid("dd000016-0000-4000-8000-000000000016");
            var ord17 = new Guid("dd000017-0000-4000-8000-000000000017");
            var ord18 = new Guid("dd000018-0000-4000-8000-000000000018");
            var ord19 = new Guid("dd000019-0000-4000-8000-000000000019");
            var ord20 = new Guid("dd000020-0000-4000-8000-000000000020");

            db.Orders.AddRange(
                // ──────────────── DELIVERED (5) ────────────────
                new Order { OrderId = ord01, AccountId = cust01, ShopId = shop01, SubtotalCents = 25000, TotalAmountCents = 25300, CommissionRate = 0.06M, CommissionCents = 1500, VoucherId = null, Status = OrderStatus.DELIVERED, DeliveryAddress = "123 Lý Thường Kiệt, Q.10, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-14), UpdatedAt = DateTime.UtcNow.AddDays(-10) },
                new Order { OrderId = ord02, AccountId = cust02, ShopId = shop02, SubtotalCents = 38000, TotalAmountCents = 36300, CommissionRate = 0.06M, CommissionCents = 2280, VoucherId = vWelcome10, Status = OrderStatus.DELIVERED, DeliveryAddress = "45 Nguyễn Huệ, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-12), UpdatedAt = DateTime.UtcNow.AddDays(-8) },
                new Order { OrderId = ord03, AccountId = cust01, ShopId = shop01, SubtotalCents = 43000, TotalAmountCents = 43000, CommissionRate = 0.06M, CommissionCents = 2580, VoucherId = vShipFree, Status = OrderStatus.DELIVERED, DeliveryAddress = "123 Lý Thường Kiệt, Q.10, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-11), UpdatedAt = DateTime.UtcNow.AddDays(-7) },
                new Order { OrderId = ord04, AccountId = cust03, ShopId = shop01, SubtotalCents = 32000, TotalAmountCents = 30000, CommissionRate = 0.06M, CommissionCents = 1920, VoucherId = vWelcome10, Status = OrderStatus.DELIVERED, DeliveryAddress = "78 Trần Hưng Đạo, Q.5, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow.AddDays(-6) },
                new Order { OrderId = ord05, AccountId = seller01, ShopId = shop02, SubtotalCents = 56000, TotalAmountCents = 50500, CommissionRate = 0.06M, CommissionCents = 3360, VoucherId = vShop220, Status = OrderStatus.DELIVERED, DeliveryAddress = "5 Đinh Tiên Hoàng, Q.Bình Thạnh, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-9), UpdatedAt = DateTime.UtcNow.AddDays(-5) },
                // ──────────────── SHIPPED (3) ────────────────
                new Order { OrderId = ord06, AccountId = seller02, ShopId = shop01, SubtotalCents = 22000, TotalAmountCents = 22300, CommissionRate = 0.06M, CommissionCents = 1320, VoucherId = null, Status = OrderStatus.SHIPPED, DeliveryAddress = "12 Lê Lợi, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-8), UpdatedAt = DateTime.UtcNow.AddDays(-3) },
                new Order { OrderId = ord07, AccountId = cust01, ShopId = shop02, SubtotalCents = 25500, TotalAmountCents = 23800, CommissionRate = 0.06M, CommissionCents = 1530, VoucherId = vWelcome10, Status = OrderStatus.SHIPPED, DeliveryAddress = "123 Lý Thường Kiệt, Q.10, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow.AddDays(-2) },
                new Order { OrderId = ord08, AccountId = cust02, ShopId = shop01, SubtotalCents = 25000, TotalAmountCents = 25300, CommissionRate = 0.06M, CommissionCents = 1500, VoucherId = null, Status = OrderStatus.SHIPPED, DeliveryAddress = "45 Nguyễn Huệ, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                // ──────────────── CONFIRMED (2) ────────────────
                new Order { OrderId = ord09, AccountId = cust03, ShopId = shop02, SubtotalCents = 50000, TotalAmountCents = 44000, CommissionRate = 0.06M, CommissionCents = 3000, VoucherId = vShop220, Status = OrderStatus.CONFIRMED, DeliveryAddress = "78 Trần Hưng Đạo, Q.5, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow.AddHours(-12) },
                new Order { OrderId = ord10, AccountId = seller01, ShopId = shop01, SubtotalCents = 58000, TotalAmountCents = 54700, CommissionRate = 0.06M, CommissionCents = 3480, VoucherId = vShop115, Status = OrderStatus.CONFIRMED, DeliveryAddress = "5 Đinh Tiên Hoàng, Q.Bình Thạnh, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-4), UpdatedAt = DateTime.UtcNow.AddHours(-6) },
                // ──────────────── PROCESSING (1) ────────────────
                new Order { OrderId = ord11, AccountId = seller02, ShopId = shop02, SubtotalCents = 17000, TotalAmountCents = 17300, CommissionRate = 0.06M, CommissionCents = 1020, VoucherId = null, Status = OrderStatus.PROCESSING, DeliveryAddress = "12 Lê Lợi, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow.AddHours(-3) },
                // ──────────────── CANCELLED (1) ────────────────
                new Order { OrderId = ord12, AccountId = cust02, ShopId = shop01, SubtotalCents = 50000, TotalAmountCents = 46900, CommissionRate = 0.06M, CommissionCents = 3000, VoucherId = vShop115, Status = OrderStatus.CANCELLED, DeliveryAddress = "45 Nguyễn Huệ, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow.AddHours(-20) },
                // ──────────────── REFUNDED (1) ────────────────
                new Order { OrderId = ord13, AccountId = cust03, ShopId = shop02, SubtotalCents = 32000, TotalAmountCents = 32300, CommissionRate = 0.06M, CommissionCents = 1920, VoucherId = null, Status = OrderStatus.REFUNDED, DeliveryAddress = "78 Trần Hưng Đạo, Q.5, TP.HCM", CreatedAt = DateTime.UtcNow.AddDays(-15), UpdatedAt = DateTime.UtcNow.AddDays(-5) },
                // ──────────────── PENDING (7) ────────────────
                new Order { OrderId = ord14, AccountId = cust01, ShopId = shop01, SubtotalCents = 43000, TotalAmountCents = 43300, CommissionRate = 0.06M, CommissionCents = 2580, VoucherId = null, Status = OrderStatus.PENDING, DeliveryAddress = "123 Lý Thường Kiệt, Q.10, TP.HCM", CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new Order { OrderId = ord15, AccountId = cust02, ShopId = shop02, SubtotalCents = 76000, TotalAmountCents = 74500, CommissionRate = 0.06M, CommissionCents = 4560, VoucherId = vWelcome10, Status = OrderStatus.PENDING, DeliveryAddress = "45 Nguyễn Huệ, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddHours(-4) },
                new Order { OrderId = ord16, AccountId = cust03, ShopId = shop01, SubtotalCents = 32000, TotalAmountCents = 32000, CommissionRate = 0.06M, CommissionCents = 1920, VoucherId = vShipFree, Status = OrderStatus.PENDING, DeliveryAddress = "78 Trần Hưng Đạo, Q.5, TP.HCM", CreatedAt = DateTime.UtcNow.AddHours(-3) },
                new Order { OrderId = ord17, AccountId = seller01, ShopId = shop02, SubtotalCents = 44000, TotalAmountCents = 44300, CommissionRate = 0.06M, CommissionCents = 2640, VoucherId = null, Status = OrderStatus.PENDING, DeliveryAddress = "5 Đinh Tiên Hoàng, Q.Bình Thạnh, TP.HCM", CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new Order { OrderId = ord18, AccountId = seller02, ShopId = shop01, SubtotalCents = 69000, TotalAmountCents = 65400, CommissionRate = 0.06M, CommissionCents = 4140, VoucherId = vShop115, Status = OrderStatus.PENDING, DeliveryAddress = "12 Lê Lợi, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Order { OrderId = ord19, AccountId = cust01, ShopId = shop02, SubtotalCents = 38000, TotalAmountCents = 38300, CommissionRate = 0.06M, CommissionCents = 2280, VoucherId = null, Status = OrderStatus.PENDING, DeliveryAddress = "123 Lý Thường Kiệt, Q.10, TP.HCM", CreatedAt = DateTime.UtcNow.AddMinutes(-45) },
                new Order { OrderId = ord20, AccountId = cust02, ShopId = shop01, SubtotalCents = 57000, TotalAmountCents = 55300, CommissionRate = 0.06M, CommissionCents = 3420, VoucherId = vWelcome10, Status = OrderStatus.PENDING, DeliveryAddress = "45 Nguyễn Huệ, Q.1, TP.HCM", CreatedAt = DateTime.UtcNow.AddMinutes(-20) }
            );
            await db.SaveChangesAsync();

            // ── ORDER ITEMS (29 items across 20 orders)
            // FulfillmentStatus maps: DELIVERED→DELIVERED, SHIPPED→SHIPPED,
            //   CONFIRMED→PACKED, PROCESSING→PICKED, CANCELLED→CANCELLED,
            //   REFUNDED→RETURNED, PENDING→UNFULFILLED
            var oi = 0;
            Guid OiId() => new Guid($"ee{++oi:D6}-0000-4000-8000-{oi:D12}");

            db.OrderItems.AddRange(
                // O01 – sofaV1×1 – subtotal=25000
                new OrderItem { OrderItemId = OiId(), OrderId = ord01, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-14) },
                // O02 – tableV2×1 – subtotal=38000
                new OrderItem { OrderItemId = OiId(), OrderId = ord02, VersionId = tableV2, UnitPriceCents = 38000, Quantity = 1, LineTotalCents = 38000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                // O03 – bedV1×1 + sofaV1×1 – subtotal=43000
                new OrderItem { OrderItemId = OiId(), OrderId = ord03, VersionId = bedV1, UnitPriceCents = 18000, Quantity = 1, LineTotalCents = 18000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-11) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord03, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-11) },
                // O04 – sofaV2×1 – subtotal=32000
                new OrderItem { OrderItemId = OiId(), OrderId = ord04, VersionId = sofaV2, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                // O05 – deskV2×2 + tableV1×1 – subtotal=56000
                new OrderItem { OrderItemId = OiId(), OrderId = ord05, VersionId = deskV2, UnitPriceCents = 12000, Quantity = 2, LineTotalCents = 24000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-9) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord05, VersionId = tableV1, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.DELIVERED, CreatedAt = DateTime.UtcNow.AddDays(-9) },
                // O06 – bedV2×1 – subtotal=22000
                new OrderItem { OrderItemId = OiId(), OrderId = ord06, VersionId = bedV2, UnitPriceCents = 22000, Quantity = 1, LineTotalCents = 22000, Status = FulfillmentStatus.SHIPPED, CreatedAt = DateTime.UtcNow.AddDays(-8) },
                // O07 – deskV1×3 – subtotal=25500
                new OrderItem { OrderItemId = OiId(), OrderId = ord07, VersionId = deskV1, UnitPriceCents = 8500, Quantity = 3, LineTotalCents = 25500, Status = FulfillmentStatus.SHIPPED, CreatedAt = DateTime.UtcNow.AddDays(-7) },
                // O08 – sofaV1×1 – subtotal=25000
                new OrderItem { OrderItemId = OiId(), OrderId = ord08, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.SHIPPED, CreatedAt = DateTime.UtcNow.AddDays(-6) },
                // O09 – tableV2×1 + deskV2×1 – subtotal=50000
                new OrderItem { OrderItemId = OiId(), OrderId = ord09, VersionId = tableV2, UnitPriceCents = 38000, Quantity = 1, LineTotalCents = 38000, Status = FulfillmentStatus.PACKED, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord09, VersionId = deskV2, UnitPriceCents = 12000, Quantity = 1, LineTotalCents = 12000, Status = FulfillmentStatus.PACKED, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                // O10 – bedV1×2 + bedV2×1 – subtotal=58000
                new OrderItem { OrderItemId = OiId(), OrderId = ord10, VersionId = bedV1, UnitPriceCents = 18000, Quantity = 2, LineTotalCents = 36000, Status = FulfillmentStatus.PACKED, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord10, VersionId = bedV2, UnitPriceCents = 22000, Quantity = 1, LineTotalCents = 22000, Status = FulfillmentStatus.PACKED, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                // O11 – deskV1×2 – subtotal=17000
                new OrderItem { OrderItemId = OiId(), OrderId = ord11, VersionId = deskV1, UnitPriceCents = 8500, Quantity = 2, LineTotalCents = 17000, Status = FulfillmentStatus.PICKED, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                // O12 – sofaV2×1 + bedV1×1 – subtotal=50000
                new OrderItem { OrderItemId = OiId(), OrderId = ord12, VersionId = sofaV2, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.CANCELLED, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord12, VersionId = bedV1, UnitPriceCents = 18000, Quantity = 1, LineTotalCents = 18000, Status = FulfillmentStatus.CANCELLED, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                // O13 – tableV1×1 – subtotal=32000
                new OrderItem { OrderItemId = OiId(), OrderId = ord13, VersionId = tableV1, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.RETURNED, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                // O14 – sofaV1×1 + bedV1×1 – subtotal=43000
                new OrderItem { OrderItemId = OiId(), OrderId = ord14, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord14, VersionId = bedV1, UnitPriceCents = 18000, Quantity = 1, LineTotalCents = 18000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-5) },
                // O15 – tableV2×2 – subtotal=76000
                new OrderItem { OrderItemId = OiId(), OrderId = ord15, VersionId = tableV2, UnitPriceCents = 38000, Quantity = 2, LineTotalCents = 76000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-4) },
                // O16 – sofaV2×1 – subtotal=32000
                new OrderItem { OrderItemId = OiId(), OrderId = ord16, VersionId = sofaV2, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-3) },
                // O17 – deskV2×1 + tableV1×1 – subtotal=44000
                new OrderItem { OrderItemId = OiId(), OrderId = ord17, VersionId = deskV2, UnitPriceCents = 12000, Quantity = 1, LineTotalCents = 12000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord17, VersionId = tableV1, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                // O18 – bedV2×2 + sofaV1×1 – subtotal=69000
                new OrderItem { OrderItemId = OiId(), OrderId = ord18, VersionId = bedV2, UnitPriceCents = 22000, Quantity = 2, LineTotalCents = 44000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord18, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddHours(-1) },
                // O19 – tableV2×1 – subtotal=38000
                new OrderItem { OrderItemId = OiId(), OrderId = ord19, VersionId = tableV2, UnitPriceCents = 38000, Quantity = 1, LineTotalCents = 38000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddMinutes(-45) },
                // O20 – sofaV1×1 + sofaV2×1 – subtotal=57000
                new OrderItem { OrderItemId = OiId(), OrderId = ord20, VersionId = sofaV1, UnitPriceCents = 25000, Quantity = 1, LineTotalCents = 25000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddMinutes(-20) },
                new OrderItem { OrderItemId = OiId(), OrderId = ord20, VersionId = sofaV2, UnitPriceCents = 32000, Quantity = 1, LineTotalCents = 32000, Status = FulfillmentStatus.UNFULFILLED, CreatedAt = DateTime.UtcNow.AddMinutes(-20) }
            );
            await db.SaveChangesAsync();

            // ══════════════════════════════════════════════════════════════════
            // 5. VOUCHER USAGES  (15 records – 1 per voucher per order)
            //   Discount calc confirmed:
            //     WELCOME10 (10%, max=2000): subtotal×0.10 capped at 2000
            //     SHOP1_15  (15%, max=3600): subtotal×0.15 capped at 3600
            //     SHOP2_20  (20%, max=6000): subtotal×0.20 capped at 6000
            //     SHIP_FREE               : full shipping fee waived
            // ══════════════════════════════════════════════════════════════════
            var vu = 0;
            Guid VuId() => new Guid($"ff{++vu:D6}-0000-4000-8000-{vu:D12}");

            db.VoucherUsages.AddRange(
                // O02 – WELCOME10 – cust02 – discount=2000  (38000×10%=3800, cap=2000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vWelcome10, AccountId = cust02, OrderId = ord02, DiscountAppliedCents = 2000, UsedAt = DateTime.UtcNow.AddDays(-12) },
                // O03 – SHIP_FREE – cust01 – discount=300   (standard shipping waived)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShipFree, AccountId = cust01, OrderId = ord03, DiscountAppliedCents = SHIP_STD, UsedAt = DateTime.UtcNow.AddDays(-11) },
                // O04 – WELCOME10 – cust03 – discount=2000  (32000×10%=3200, cap=2000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vWelcome10, AccountId = cust03, OrderId = ord04, DiscountAppliedCents = 2000, UsedAt = DateTime.UtcNow.AddDays(-10) },
                // O04 – SHIP_FREE – cust03 – discount=300   (standard shipping waived)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShipFree, AccountId = cust03, OrderId = ord04, DiscountAppliedCents = SHIP_STD, UsedAt = DateTime.UtcNow.AddDays(-10) },
                // O05 – SHOP2_20 – seller01 – discount=6000 (56000×20%=11200, cap=6000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShop220, AccountId = seller01, OrderId = ord05, DiscountAppliedCents = 6000, UsedAt = DateTime.UtcNow.AddDays(-9) },
                // O07 – WELCOME10 – cust01 – discount=2000  (25500×10%=2550, cap=2000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vWelcome10, AccountId = cust01, OrderId = ord07, DiscountAppliedCents = 2000, UsedAt = DateTime.UtcNow.AddDays(-7) },
                // O09 – SHOP2_20 – cust03 – discount=6000   (50000×20%=10000, cap=6000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShop220, AccountId = cust03, OrderId = ord09, DiscountAppliedCents = 6000, UsedAt = DateTime.UtcNow.AddDays(-5) },
                // O09 – SHIP_FREE – cust03 – discount=500   (express shipping waived)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShipFree, AccountId = cust03, OrderId = ord09, DiscountAppliedCents = SHIP_EXP, UsedAt = DateTime.UtcNow.AddDays(-5) },
                // O10 – SHOP1_15 – seller01 – discount=3600 (58000×15%=8700, cap=3600)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShop115, AccountId = seller01, OrderId = ord10, DiscountAppliedCents = 3600, UsedAt = DateTime.UtcNow.AddDays(-4) },
                // O12 – SHOP1_15 – cust02 – discount=3600   (50000×15%=7500, cap=3600)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShop115, AccountId = cust02, OrderId = ord12, DiscountAppliedCents = 3600, UsedAt = DateTime.UtcNow.AddDays(-2) },
                // O15 – WELCOME10 – cust02 – discount=2000  (76000×10%=7600, cap=2000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vWelcome10, AccountId = cust02, OrderId = ord15, DiscountAppliedCents = 2000, UsedAt = DateTime.UtcNow.AddHours(-4) },
                // O16 – SHIP_FREE – cust03 – discount=300   (standard shipping waived)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShipFree, AccountId = cust03, OrderId = ord16, DiscountAppliedCents = SHIP_STD, UsedAt = DateTime.UtcNow.AddHours(-3) },
                // O18 – SHOP1_15 – seller02 – discount=3600 (69000×15%=10350, cap=3600)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShop115, AccountId = seller02, OrderId = ord18, DiscountAppliedCents = 3600, UsedAt = DateTime.UtcNow.AddHours(-1) },
                // O18 – SHIP_FREE – seller02 – discount=500 (express shipping waived)
                new VoucherUsage { UsageId = VuId(), VoucherId = vShipFree, AccountId = seller02, OrderId = ord18, DiscountAppliedCents = SHIP_EXP, UsedAt = DateTime.UtcNow.AddHours(-1) },
                // O20 – WELCOME10 – cust02 – discount=2000  (57000×10%=5700, cap=2000)
                new VoucherUsage { UsageId = VuId(), VoucherId = vWelcome10, AccountId = cust02, OrderId = ord20, DiscountAppliedCents = 2000, UsedAt = DateTime.UtcNow.AddMinutes(-20) }
            );
            await db.SaveChangesAsync();
        }
    }
}

