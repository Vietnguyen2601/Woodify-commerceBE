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
            var shopId = Guid.NewGuid(); // Sample shop ID
            
            dbContext.Orders.AddRange(
                new Order 
                { 
                    OrderId = Guid.NewGuid(), 
                    AccountId = Guid.NewGuid(), 
                    ShopId = shopId, 
                    SubtotalCents = 9995000.00, 
                    TotalAmountCents = 10295000.00, 
                    Payment = Guid.NewGuid(), 
                    Status = OrderStatus.DELIVERED, 
                    DeliveryAddressId = "Address-001",
                    CreatedAt = DateTime.UtcNow.AddDays(-5), 
                    UpdatedAt = DateTime.UtcNow.AddDays(-1) 
                },
                new Order 
                { 
                    OrderId = Guid.NewGuid(), 
                    AccountId = Guid.NewGuid(), 
                    ShopId = shopId, 
                    SubtotalCents = 2495000.00, 
                    TotalAmountCents = 2620000.00, 
                    Payment = Guid.NewGuid(), 
                    Status = OrderStatus.SHIPPED, 
                    DeliveryAddressId = "Address-002",
                    CreatedAt = DateTime.UtcNow.AddDays(-2), 
                    UpdatedAt = DateTime.UtcNow.AddDays(-1) 
                },
                new Order 
                { 
                    OrderId = Guid.NewGuid(), 
                    AccountId = Guid.NewGuid(), 
                    ShopId = shopId, 
                    SubtotalCents = 14990000.00, 
                    TotalAmountCents = 15490000.00, 
                    VoucherId = Guid.NewGuid(), 
                    Payment = Guid.NewGuid(), 
                    Status = OrderStatus.CONFIRMED, 
                    DeliveryAddressId = "Address-003",
                    CreatedAt = DateTime.UtcNow.AddHours(-3), 
                    UpdatedAt = DateTime.UtcNow.AddHours(-2) 
                },
                new Order 
                { 
                    OrderId = Guid.NewGuid(), 
                    AccountId = Guid.NewGuid(), 
                    ShopId = shopId, 
                    SubtotalCents = 7495000.00, 
                    TotalAmountCents = 7645000.00, 
                    Status = OrderStatus.PENDING, 
                    DeliveryAddressId = "Address-004",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new Order 
                { 
                    OrderId = Guid.NewGuid(), 
                    AccountId = Guid.NewGuid(), 
                    ShopId = shopId, 
                    SubtotalCents = 3995000.00, 
                    TotalAmountCents = 4120000.00, 
                    Payment = Guid.NewGuid(), 
                    Status = OrderStatus.REFUNDED, 
                    DeliveryAddressId = "Address-005",
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
                        Price = 1000000.00 + (i * 500000.00)
                    });
                    items.Add(new CartItem 
                    { 
                        CartItemId = Guid.NewGuid(), 
                        CartId = cartIds[i], 
                        VersionId = Guid.NewGuid(), 
                        ShopId = shopId, 
                        Quantity = 2, 
                        Price = 2000000.00 + (i * 700000.00)
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
                    var unitPriceCents = (1000000 + i * 500000) * 100L;
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
                    VersionName = "Phiên bản tiêu chuẩn",
                    Price = 2999000.00,
                    Currency = "VND",
                    StockQuantity = 50,
                    WeightGrams = 15000,
                    LengthCm = 60,
                    WidthCm = 55,
                    HeightCm = 90,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
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
                    Price = 4999000.00,
                    Currency = "VND",
                    StockQuantity = 30,
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 70,
                    HeightCm = 75,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
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
                    Price = 5999000.00,
                    Currency = "VND",
                    StockQuantity = 20,
                    WeightGrams = 80000,
                    LengthCm = 200,
                    WidthCm = 160,
                    HeightCm = 45,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
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
                    Price = 7999000.00,
                    Currency = "VND",
                    StockQuantity = 15,
                    WeightGrams = 45000,
                    LengthCm = 100,
                    WidthCm = 40,
                    HeightCm = 180,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
