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
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Nguyễn Văn Shop",
                    Description = "Cửa hàng gỗ chất lượng cao với mục đích cung cấp nội thất gỗ tự nhiên",
                    Rating = 4.5m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Trần Thị Antique",
                    Description = "Chuyên cung cấp những sản phẩm gỗ vintage và trang trí hiện đại",
                    Rating = 4.8m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    ShopId = Guid.NewGuid(),
                    OwnerAccountId = Guid.NewGuid(),
                    Name = "Gỗ Tự Nhiên Premium",
                    Description = "Bán các sản phẩm gỗ tự nhiên nhập khẩu từ châu Âu",
                    Rating = 4.7m,
                    Status = ShopService.Domain.Enums.ShopStatus.ACTIVE,
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
