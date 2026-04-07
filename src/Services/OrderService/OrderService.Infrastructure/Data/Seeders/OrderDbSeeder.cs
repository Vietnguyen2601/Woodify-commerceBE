using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Data.Seeders;

public static class OrderDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as OrderService.Infrastructure.Data.Context.OrderDbContext;
        if (dbContext == null) return;

        // Customer account IDs from IdentityService seeder
        var customer01Id = new Guid("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f");
        var customer02Id = new Guid("d2e3f4a5-b6c7-4d8e-9f0a-1b2c3d4e5f6a");
        var customer03Id = new Guid("e3f4a5b6-c7d8-4e9f-0a1b-2c3d4e5f6a7b");

        // Shop IDs from ShopService seeder
        var shop01Id = new Guid("f4a5b6c7-d8e9-4f0a-1b2c-3d4e5f6a7b8c");
        var shop02Id = new Guid("a5b6c7d8-e9f0-4a1b-2c3d-4e5f6a7b8c9d");

        // Product version IDs - from ProductService seeder
        var sofaV1 = new Guid("f0a1b2c3-d4e5-4f6a-7b8c-9d0e1f2a3b4c");
        var sofaV2 = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
        var bedV1 = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e");
        var bedV2 = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f");
        var tableV1 = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a");
        var tableV2 = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b");
        var deskV1 = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c");
        var deskV2 = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d");

        if (!dbContext.Carts.Any())
        {
            dbContext.Carts.AddRange(
                // Customer 01 - Interest in Sofa
                new Cart
                {
                    CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                    AccountId = customer01Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CartItems = new List<CartItem>
                    {
                        new CartItem
                        {
                            CartItemId = new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"),
                            CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                            VersionId = sofaV1,
                            ShopId = shop01Id,
                            Quantity = 1,
                            Price = 25000.00
                        },
                        new CartItem
                        {
                            CartItemId = new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"),
                            CartId = new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"),
                            VersionId = bedV1,
                            ShopId = shop01Id,
                            Quantity = 1,
                            Price = 18000.00
                        }
                    }
                },
                // Customer 02 - Interest in Table
                new Cart
                {
                    CartId = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"),
                    AccountId = customer02Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    CartItems = new List<CartItem>
                    {
                        new CartItem
                        {
                            CartItemId = new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"),
                            CartId = new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"),
                            VersionId = tableV2,
                            ShopId = shop02Id,
                            Quantity = 1,
                            Price = 38000.00
                        }
                    }
                },
                // Customer 03 - Interest in Desk and Sofa Premium
                new Cart
                {
                    CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"),
                    AccountId = customer03Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CartItems = new List<CartItem>
                    {
                        new CartItem
                        {
                            CartItemId = new Guid("b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e"),
                            CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"),
                            VersionId = deskV2,
                            ShopId = shop02Id,
                            Quantity = 2,
                            Price = 120000.00
                        },
                        new CartItem
                        {
                            CartItemId = new Guid("c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f"),
                            CartId = new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"),
                            VersionId = sofaV2,
                            ShopId = shop01Id,
                            Quantity = 1,
                            Price = 320000.00
                        }
                    }
                }
            );
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.Orders.Any())
        {
            var shopId = Guid.NewGuid(); // Sample shop ID

            dbContext.Orders.AddRange(
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    ShopId = shopId,
                    SubtotalCents = 99950.00,
                    TotalAmountCents = 102950.00,
                    Status = OrderStatus.DELIVERED,
                    DeliveryAddress = "Address-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    ShopId = shopId,
                    SubtotalCents = 249500.00,
                    TotalAmountCents = 262000.00,
                    Status = OrderStatus.SHIPPED,
                    DeliveryAddress = "Address-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    ShopId = shopId,
                    SubtotalCents = 149900.00,
                    TotalAmountCents = 154900.00,
                    VoucherId = Guid.NewGuid(),
                    Status = OrderStatus.CONFIRMED,
                    DeliveryAddress = "Address-003",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    ShopId = shopId,
                    SubtotalCents = 74950.00,
                    TotalAmountCents = 76450.00,
                    Status = OrderStatus.PENDING,
                    DeliveryAddress = "Address-004",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    ShopId = shopId,
                    SubtotalCents = 399500.00,
                    TotalAmountCents = 412000.00,
                    Status = OrderStatus.REFUNDED,
                    DeliveryAddress = "Address-005",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                }
            );
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.CartItems.Any())
        {
            var cartIds = await dbContext.Carts.Select(c => c.CartId).ToListAsync();
            if (cartIds.Count > 0)
            {
                var items = new List<CartItem>();
                var shopId = Guid.NewGuid(); // Sample shop ID

                for (int i = 0; i < cartIds.Count; i++)
                {
                    items.Add(new CartItem
                    {
                        CartItemId = Guid.NewGuid(),
                        CartId = cartIds[i],
                        VersionId = Guid.NewGuid(),
                        ShopId = shopId,
                        Quantity = i + 1,
                        Price = 100000.00 + (i * 50000.00)
                    });
                    items.Add(new CartItem
                    {
                        CartItemId = Guid.NewGuid(),
                        CartId = cartIds[i],
                        VersionId = Guid.NewGuid(),
                        ShopId = shopId,
                        Quantity = 2,
                        Price = 200000.00 + (i * 70000.00)
                    });
                }
                dbContext.CartItems.AddRange(items);
                await dbContext.SaveChangesAsync();
            }
        }

        if (!dbContext.OrderItems.Any())
        {
            var orderIds = await dbContext.Orders.Select(o => o.OrderId).ToListAsync();
            if (orderIds.Count > 0)
            {
                var items = new List<OrderItem>();
                for (int i = 0; i < orderIds.Count; i++)
                {
                    var unitPriceCents = (1000 + i * 500000) * 100L;
                    var quantity = i + 1;
                    var lineTotalCents = (double)(unitPriceCents * quantity);
                    items.Add(new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = orderIds[i],
                        VersionId = Guid.NewGuid(),
                        Quantity = quantity,
                        UnitPriceCents = unitPriceCents,
                        LineTotalCents = lineTotalCents,
                        Status = FulfillmentStatus.UNFULFILLED,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                dbContext.OrderItems.AddRange(items);
                await dbContext.SaveChangesAsync();
            }
        }

        if (!dbContext.ProductVersionCaches.Any())
        {
            dbContext.ProductVersionCaches.AddRange(
                // Seller 01 - Sofa (2 versions)
                new ProductVersionCache
                {
                    VersionId = new Guid("f0a1b2c3-d4e5-4f6a-7b8c-9d0e1f2a3b4c"),
                    ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"),
                    ShopId = shop01Id,
                    ProductName = "Ghế Sofa Gỗ Óc Chó Cao Cấp V1",
                    ProductDescription = "Ghế sofa phiên bản tiêu chuẩn từ gỗ óc chó",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "SOFA-OC-CHO-V1",
                    VersionNumber = 1,
                    VersionName = "Phiên bản tiêu chuẩn",
                    Price = 25000.00,
                    Currency = "VND",
                    StockQuantity = 10,
                    WoodType = "Óc Chó",
                    WeightGrams = 85000,
                    LengthCm = 200,
                    WidthCm = 90,
                    HeightCm = 80,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                new ProductVersionCache
                {
                    VersionId = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                    ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"),
                    ShopId = shop01Id,
                    ProductName = "Ghế Sofa Gỗ Óc Chó Cao Cấp V2",
                    ProductDescription = "Ghế sofa phiên bản sang trọng từ gỗ óc chó",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "SOFA-OC-CHO-V2",
                    VersionNumber = 2,
                    VersionName = "Phiên bản sang trọng",
                    Price = 32000.00,
                    Currency = "VND",
                    StockQuantity = 5,
                    WoodType = "Óc Chó",
                    WeightGrams = 90000,
                    LengthCm = 200,
                    WidthCm = 100,
                    HeightCm = 85,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                // Seller 01 - Bed (2 versions)
                new ProductVersionCache
                {
                    VersionId = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"),
                    ProductId = new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"),
                    ShopId = shop01Id,
                    ProductName = "Giường Ngủ Gỗ Sồi 1m8 V1",
                    ProductDescription = "Giường ngủ gỗ sồi có ngăn kéo",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "BED-SOI-1M8-V1",
                    VersionNumber = 1,
                    VersionName = "Giường có ngăn kéo",
                    Price = 18000.00,
                    Currency = "VND",
                    StockQuantity = 15,
                    WoodType = "Sồi",
                    WeightGrams = 120000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 40,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                new ProductVersionCache
                {
                    VersionId = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"),
                    ProductId = new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"),
                    ShopId = shop01Id,
                    ProductName = "Giường Ngủ Gỗ Sồi 1m8 V2",
                    ProductDescription = "Giường ngủ cao cấp ngoại nhập",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "BED-SOI-1M8-V2",
                    VersionNumber = 2,
                    VersionName = "Giường cao cấp ngoại nhập",
                    Price = 22000.00,
                    Currency = "VND",
                    StockQuantity = 8,
                    WoodType = "Sồi",
                    WeightGrams = 125000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 45,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                // Seller 02 - Table (2 versions)
                new ProductVersionCache
                {
                    VersionId = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"),
                    ProductId = new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"),
                    ShopId = shop02Id,
                    ProductName = "Bàn Ăn Gỗ Teak 6 Ghế V1",
                    ProductDescription = "Combo bàn ăn 6 ghế gỗ teak",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "TABLE-TEAK-6G-V1",
                    VersionNumber = 1,
                    VersionName = "Combo bàn + 6 ghế",
                    Price = 32000.00,
                    Currency = "VND",
                    StockQuantity = 8,
                    WoodType = "Teak",
                    WeightGrams = 95000,
                    LengthCm = 160,
                    WidthCm = 90,
                    HeightCm = 75,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                new ProductVersionCache
                {
                    VersionId = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"),
                    ProductId = new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"),
                    ShopId = shop02Id,
                    ProductName = "Bàn Ăn Gỗ Teak 6 Ghế V2",
                    ProductDescription = "Combo bàn ăn mở rộng gỗ teak",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "TABLE-TEAK-6G-V2",
                    VersionNumber = 2,
                    VersionName = "Combo bàn ăn mở rộng",
                    Price = 38000.00,
                    Currency = "VND",
                    StockQuantity = 5,
                    WoodType = "Teak",
                    WeightGrams = 100000,
                    LengthCm = 180,
                    WidthCm = 100,
                    HeightCm = 75,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                // Seller 02 - Desk (2 versions)
                new ProductVersionCache
                {
                    VersionId = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"),
                    ProductId = new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"),
                    ShopId = shop02Id,
                    ProductName = "Bàn Làm Việc Gỗ Ash Hiện Đại V1",
                    ProductDescription = "Bàn làm việc gỗ ash phong cách Bắc Âu",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "DESK-ASH-MOD-V1",
                    VersionNumber = 1,
                    VersionName = "Bàn có ngăn kéo và giá sách",
                    Price = 8500.00,
                    Currency = "VND",
                    StockQuantity = 20,
                    WoodType = "Ash",
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 60,
                    HeightCm = 75,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                },
                new ProductVersionCache
                {
                    VersionId = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"),
                    ProductId = new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"),
                    ShopId = shop02Id,
                    ProductName = "Bàn Làm Việc Gỗ Ash Hiện Đại V2",
                    ProductDescription = "Bàn làm việc Premium gỗ ash",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "DESK-ASH-MOD-V2",
                    VersionNumber = 2,
                    VersionName = "Bàn làm việc Premium",
                    Price = 12000.00,
                    Currency = "VND",
                    StockQuantity = 12,
                    WoodType = "Ash",
                    WeightGrams = 38000,
                    LengthCm = 160,
                    WidthCm = 70,
                    HeightCm = 75,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
