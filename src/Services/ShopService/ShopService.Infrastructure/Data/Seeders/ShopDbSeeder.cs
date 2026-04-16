using ShopService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ShopService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder cho ShopService Database
/// Khởi tạo dữ liệu Shops mặc định
/// </summary>
public static class ShopDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as ShopService.Infrastructure.Data.Context.ShopDbContext;
        if (dbContext == null)
            return;

        // Seed Shops nếu chưa có
        if (!dbContext.Shops.Any())
        {
            var shops = new List<Shop>
            {
                // Shop của Seller 01
                new()
                {
                    ShopId = new Guid("f4a5b6c7-d8e9-4f0a-1b2c-3d4e5f6a7b8c"),
                    OwnerAccountId = new Guid("a7b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d"),
                    Name = "Nội Thất Gỗ Cao Cấp A",
                    Description = "Cửa hàng gỗ chất lượng cao với mục đích cung cấp nội thất gỗ tự nhiên cao cấp từ các loại gỗ nhập khẩu",
                    Rating = 4.7m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    DefaultProvider = null, // Will be set after seeding providers
                    BankName = "Vietcombank",
                    BankAccountNumber = "1001234567",
                    BankAccountName = "NGUYEN VAN A",
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                },
                // Shop của Seller 02
                new()
                {
                    ShopId = new Guid("a5b6c7d8-e9f0-4a1b-2c3d-4e5f6a7b8c9d"),
                    OwnerAccountId = new Guid("b8c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e"),
                    Name = "Nội Thất Gỗ Hiện Đại B",
                    Description = "Chuyên cung cấp những sản phẩm gỗ vintage, hiện đại và trang trí nội thất theo phong cách Bắc Âu",
                    Rating = 4.8m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    DefaultProvider = null, // Will be set after seeding providers
                    BankName = "BIDV",
                    BankAccountNumber = "2005678901",
                    BankAccountName = "TRAN THI B",
                    CreatedAt = DateTime.UtcNow.AddDays(-50)
                },
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Gỗ Tự Nhiên Premium",
                    Description = "Bán các sản phẩm gỗ tự nhiên nhập khẩu từ châu Âu",
                    Rating = 4.7m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    BankName = "Techcombank",
                    BankAccountNumber = "3009123456",
                    BankAccountName = "PHAM VAN C",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Nội Thất Sang Trọng",
                    Description = "Thiết kế và cung cấp nội thất gỗ cao cấp theo yêu cầu",
                    Rating = 4.9m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    BankName = "ACB",
                    BankAccountNumber = "4017654321",
                    BankAccountName = "LE THI D",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Gỗ Lâm Đồng",
                    Description = "Chuyên bán sản phẩm gỗ từ Lâm Đồng, chất lượng tốt giá hợp lý",
                    Rating = 4.3m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    BankName = "Agribank",
                    BankAccountNumber = "5050789012",
                    BankAccountName = "HOANG VAN E",
                    CreatedAt = DateTime.UtcNow
                }
            };

            dbContext.Shops.AddRange(shops);
            await dbContext.SaveChangesAsync();
        }

        // Seed ShopFollowers nếu chưa có
        if (!dbContext.ShopFollowers.Any())
        {
            var shopIds = await dbContext.Shops.Select(s => s.ShopId).ToListAsync();
            if (shopIds.Count > 0)
            {
                var random = new Random();
                var followers = new List<ShopFollower>();

                // Tạo một số followers cho mỗi shop (từ 1-5 followers/shop)
                foreach (var shopId in shopIds)
                {
                    int followerCount = random.Next(1, 6); // Random từ 1 đến 5 followers
                    for (int i = 0; i < followerCount; i++)
                    {
                        followers.Add(new ShopFollower
                        {
                            ShopFollowerId = Guid.NewGuid(),
                            ShopId = shopId,
                            AccountId = Guid.NewGuid(), // Random AccountId
                            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)) // Random ngày trong 30 ngày qua
                        });
                    }
                }

                dbContext.ShopFollowers.AddRange(followers);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
