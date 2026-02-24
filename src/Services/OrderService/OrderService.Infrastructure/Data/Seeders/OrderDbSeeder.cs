using OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Data.Seeders;

public static class OrderDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as OrderService.Infrastructure.Data.Context.OrderDbContext;
        if (dbContext == null) return;

        if (!dbContext.Carts.Any())
        {
            dbContext.Carts.AddRange(
                new Cart { CartId = Guid.NewGuid(), AccountId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Cart { CartId = Guid.NewGuid(), AccountId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Cart { CartId = Guid.NewGuid(), AccountId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Cart { CartId = Guid.NewGuid(), AccountId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
                new Cart { CartId = Guid.NewGuid(), AccountId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            );
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.Orders.Any())
        {
            dbContext.Orders.AddRange(
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-001", AccountId = Guid.NewGuid(), CustomerName = "Lê Văn Anh", CustomerPhone = "0901000001", CustomerEmail = "levananh@example.com", ShopId = Guid.NewGuid(), ShopName = "Shop Nội Thất A", Currency = "VND", SubtotalCents = 199900000, ShippingFeeCents = 3000000, TotalAmountCents = 202900000, PaymentMethod = PaymentMethod.VNPAY, PaymentStatus = PaymentStatus.PAID, Status = OrderStatus.DELIVERED, PlacedAt = DateTime.UtcNow.AddDays(-5), DeliveredAt = DateTime.UtcNow.AddDays(-1), CompletedAt = DateTime.UtcNow.AddDays(-1) },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-002", AccountId = Guid.NewGuid(), CustomerName = "Trần Thị Bình", CustomerPhone = "0902000002", ShopId = Guid.NewGuid(), ShopName = "Shop Nội Thất B", Currency = "VND", SubtotalCents = 49900000, ShippingFeeCents = 2500000, TotalAmountCents = 52400000, PaymentMethod = PaymentMethod.BANK_TRANSFER, PaymentStatus = PaymentStatus.PENDING, Status = OrderStatus.SHIPPED, PlacedAt = DateTime.UtcNow.AddDays(-2), ShippedAt = DateTime.UtcNow.AddDays(-1) },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-003", AccountId = Guid.NewGuid(), CustomerName = "Phạm Văn Công", CustomerPhone = "0903000003", ShopId = Guid.NewGuid(), ShopName = "Shop Nội Thất C", Currency = "VND", SubtotalCents = 299800000, ShippingFeeCents = 5000000, TotalAmountCents = 304800000, PaymentMethod = PaymentMethod.WALLET, PaymentStatus = PaymentStatus.PAID, Status = OrderStatus.CONFIRMED, PlacedAt = DateTime.UtcNow.AddHours(-3), ConfirmedAt = DateTime.UtcNow.AddHours(-2) },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-004", AccountId = Guid.NewGuid(), CustomerName = "Hoàng Thị Dương", CustomerPhone = "0904000004", ShopId = Guid.NewGuid(), ShopName = "Shop Nội Thất D", Currency = "VND", SubtotalCents = 149900000, ShippingFeeCents = 3000000, TotalAmountCents = 152900000, PaymentMethod = PaymentMethod.BANK_TRANSFER, PaymentStatus = PaymentStatus.PENDING, Status = OrderStatus.PENDING, PlacedAt = DateTime.UtcNow.AddMinutes(-30) },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-005", AccountId = Guid.NewGuid(), CustomerName = "Ngô Quốc Gia", CustomerPhone = "0905000005", ShopId = Guid.NewGuid(), ShopName = "Shop Nội Thất E", Currency = "VND", SubtotalCents = 79900000, ShippingFeeCents = 2500000, TotalAmountCents = 82400000, PaymentMethod = PaymentMethod.VNPAY, PaymentStatus = PaymentStatus.REFUNDED, Status = OrderStatus.REFUNDED, PlacedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-3) }
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
                    items.Add(new CartItem { CartItemId = Guid.NewGuid(), CartId = cartIds[i], VersionId = Guid.NewGuid(), ShopId = shopId, Quantity = i + 1, UnitPriceCents = (100000 + i * 50000) * 100, IsActive = true, AddedAt = DateTime.UtcNow });
                    items.Add(new CartItem { CartItemId = Guid.NewGuid(), CartId = cartIds[i], VersionId = Guid.NewGuid(), ShopId = shopId, Quantity = 2, UnitPriceCents = (200000 + i * 70000) * 100, IsActive = true, AddedAt = DateTime.UtcNow });
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
                    var unitPrice = (100000 + i * 50000) * 100L;
                    var quantity = i + 1;
                    var lineTotal = unitPrice * quantity;
                    items.Add(new OrderItem { OrderItemId = Guid.NewGuid(), OrderId = orderIds[i], VersionId = Guid.NewGuid(), ProductName = $"Sản phẩm gỗ {i + 1}", SellerSku = $"SKU-{3000 + i}", Quantity = quantity, UnitPriceCents = unitPrice, LineTotalCents = lineTotal, Status = FulfillmentStatus.UNFULFILLED });
                }
                dbContext.OrderItems.AddRange(items);
                await dbContext.SaveChangesAsync();
            }
        }

        if (!dbContext.ProductVersionCaches.Any())
        {
            var shopId = Guid.NewGuid();
            
            dbContext.ProductVersionCaches.AddRange(
                new ProductVersionCache 
                { 
                    VersionId = Guid.NewGuid(), 
                    ProductId = Guid.NewGuid(), 
                    ShopId = shopId,
                    ProductName = "Ghế Gỗ Teak",
                    ProductDescription = "Ghế cao cấp từ gỗ teak nhập khẩu",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "WOOD-CHAIR-001",
                    VersionNumber = 1,
                    VersionName = "Phiên bản tiêu chuẩn",
                    PriceCents = 299900000,
                    BasePriceCents = 349900000,
                    Currency = "VND",
                    StockQuantity = 50,
                    WeightGrams = 15000,
                    LengthCm = 60,
                    WidthCm = 55,
                    HeightCm = 90,
                    BulkyType = "NORMAL",
                    IsFragile = false,
                    WarrantyMonths = 24,
                    IsActive = true,
                    IsDefault = true
                },
                new ProductVersionCache 
                { 
                    VersionId = Guid.NewGuid(), 
                    ProductId = Guid.NewGuid(), 
                    ShopId = shopId,
                    ProductName = "Bàn Làm Việc Gỗ Sồi",
                    ProductDescription = "Bàn làm việc hiện đại từ gỗ sồi tự nhiên",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "WOOD-DESK-001",
                    VersionNumber = 1,
                    PriceCents = 499900000,
                    BasePriceCents = 599900000,
                    Currency = "VND",
                    StockQuantity = 30,
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 70,
                    HeightCm = 75,
                    BulkyType = "BULKY",
                    IsFragile = false,
                    WarrantyMonths = 36,
                    IsActive = true,
                    IsDefault = true
                },
                new ProductVersionCache 
                { 
                    VersionId = Guid.NewGuid(), 
                    ProductId = Guid.NewGuid(), 
                    ShopId = shopId,
                    ProductName = "Giường Ngủ Gỗ Thông",
                    ProductDescription = "Giường ngủ cao cấp từ gỗ thông tự nhiên",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "WOOD-BED-001",
                    VersionNumber = 1,
                    PriceCents = 599900000,
                    BasePriceCents = 699900000,
                    Currency = "VND",
                    StockQuantity = 20,
                    WeightGrams = 80000,
                    LengthCm = 200,
                    WidthCm = 160,
                    HeightCm = 45,
                    BulkyType = "SUPER_BULKY",
                    IsFragile = false,
                    RequiresSpecialHandling = true,
                    WarrantyMonths = 48,
                    IsActive = true,
                    IsDefault = true
                },
                new ProductVersionCache 
                { 
                    VersionId = Guid.NewGuid(), 
                    ProductId = Guid.NewGuid(), 
                    ShopId = shopId,
                    ProductName = "Tủ Âm Tường Gỗ Óc Chó",
                    ProductDescription = "Tủ âm tường sang trọng từ gỗ óc chó",
                    ProductStatus = "PUBLISHED",
                    SellerSku = "WOOD-CABINET-001",
                    VersionNumber = 1,
                    PriceCents = 799900000,
                    BasePriceCents = 899900000,
                    Currency = "VND",
                    StockQuantity = 15,
                    WeightGrams = 45000,
                    LengthCm = 100,
                    WidthCm = 40,
                    HeightCm = 180,
                    BulkyType = "BULKY",
                    IsFragile = true,
                    WarrantyMonths = 36,
                    IsActive = true,
                    IsDefault = true
                }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
