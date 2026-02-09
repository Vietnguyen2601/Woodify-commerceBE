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
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-001", AccountId = Guid.NewGuid(), ShopId = Guid.NewGuid(), Currency = "VND", SubtotalCents = 199900000, ShippingFeeCents = 3000000, TotalAmountCents = 202900000, Status = OrderStatus.DELIVERED, PlacedAt = DateTime.UtcNow.AddDays(-5), CompletedAt = DateTime.UtcNow.AddDays(-1), CustomerName = "Lê Văn Anh", CustomerPhone = "0901000001", ShippingAddress = "123 Nguyễn Huệ, Q.1, TP.HCM" },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-002", AccountId = Guid.NewGuid(), ShopId = Guid.NewGuid(), Currency = "VND", SubtotalCents = 49900000, ShippingFeeCents = 2500000, TotalAmountCents = 52400000, Status = OrderStatus.SHIPPED, PlacedAt = DateTime.UtcNow.AddDays(-2), CustomerName = "Trần Thị Bình", CustomerPhone = "0902000002", ShippingAddress = "456 Lê Lợi, Q.5, TP.HCM" },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-003", AccountId = Guid.NewGuid(), ShopId = Guid.NewGuid(), Currency = "VND", SubtotalCents = 299800000, ShippingFeeCents = 5000000, TotalAmountCents = 304800000, Status = OrderStatus.CONFIRMED, PlacedAt = DateTime.UtcNow.AddHours(-3), CustomerName = "Phạm Văn Công", CustomerPhone = "0903000003", ShippingAddress = "789 Võ Thị Sáu, Q.3, TP.HCM" },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-004", AccountId = Guid.NewGuid(), ShopId = Guid.NewGuid(), Currency = "VND", SubtotalCents = 149900000, ShippingFeeCents = 3000000, TotalAmountCents = 152900000, Status = OrderStatus.PENDING, PlacedAt = DateTime.UtcNow.AddMinutes(-30), CustomerName = "Hoàng Thị Dương", CustomerPhone = "0904000004", ShippingAddress = "321 Đinh Tiên Hoàng, Q.1, TP.HCM" },
                new Order { OrderId = Guid.NewGuid(), OrderCode = "ORD-005", AccountId = Guid.NewGuid(), ShopId = Guid.NewGuid(), Currency = "VND", SubtotalCents = 79900000, ShippingFeeCents = 2500000, TotalAmountCents = 82400000, Status = OrderStatus.RETURNED, PlacedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-3), CustomerName = "Ngô Quốc Gia", CustomerPhone = "0905000005", ShippingAddress = "654 Pasteur, Q.1, TP.HCM" }
            );
            await dbContext.SaveChangesAsync();
        }

        if (!dbContext.CartItems.Any())
        {
            var cartIds = await dbContext.Carts.Select(c => c.CartId).ToListAsync();
            if (cartIds.Count > 0)
            {
                var items = new List<CartItem>();
                for (int i = 0; i < cartIds.Count; i++)
                {
                    items.Add(new CartItem { CartItemId = Guid.NewGuid(), CartId = cartIds[i], ProductVersionId = Guid.NewGuid(), SkuCode = $"SKU-{1000 + i}", Title = $"Sản phẩm {i + 1}", UnitPriceCents = (100000 + i * 50000) * 100, Qty = i + 1 });
                    items.Add(new CartItem { CartItemId = Guid.NewGuid(), CartId = cartIds[i], ProductVersionId = Guid.NewGuid(), SkuCode = $"SKU-{2000 + i}", Title = $"Sản phẩm {20 + i}", UnitPriceCents = (200000 + i * 70000) * 100, Qty = 2 });
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
                    var qty = i + 1;
                    var lineTotal = unitPrice * qty;
                    items.Add(new OrderItem { OrderItemId = Guid.NewGuid(), OrderId = orderIds[i], ProductId = Guid.NewGuid(), ProductVersionId = Guid.NewGuid(), SkuCode = $"SKU-{3000 + i}", Title = $"Sản phẩm gỗ {i + 1}", UnitPriceCents = unitPrice, Qty = qty, LineTotalCents = lineTotal, FulfillmentStatus = FulfillmentStatus.UNFULFILLED });
                }
                dbContext.OrderItems.AddRange(items);
                await dbContext.SaveChangesAsync();
            }
        }

        if (!dbContext.ProductVersionCaches.Any())
        {
            dbContext.ProductVersionCaches.AddRange(
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Ghế Gỗ Teak", Description = "Ghế từ gỗ teak", PriceCents = 299900000, Sku = "WOOD-CHAIR-001", ProductStatus = "PUBLISHED" },
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Bàn Làm Việc Gỗ Sồi", Description = "Bàn từ gỗ sồi", PriceCents = 499900000, Sku = "WOOD-DESK-001", ProductStatus = "PUBLISHED" },
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Giường Ngủ Gỗ Thông", Description = "Giường từ gỗ thông", PriceCents = 599900000, Sku = "WOOD-BED-001", ProductStatus = "PUBLISHED" },
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Tủ Âm Tường Gỗ Óc Chó", Description = "Tủ từ gỗ óc chó", PriceCents = 799900000, Sku = "WOOD-CABINET-001", ProductStatus = "PUBLISHED" },
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Kệ Sách Gỗ Walnut", Description = "Kệ từ gỗ walnut", PriceCents = 399900000, Sku = "WOOD-SHELF-001", ProductStatus = "PUBLISHED" },
                new ProductVersionCache { VersionId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Title = "Bàn Ăn Gỗ Hương", Description = "Bàn ăn từ gỗ hương", PriceCents = 899900000, Sku = "WOOD-TABLE-001", ProductStatus = "PUBLISHED" }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
