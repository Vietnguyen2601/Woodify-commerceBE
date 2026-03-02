using ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder cho ProductService Database
/// Khởi tạo dữ liệu Categories, ProductMasters và ProductVersions mặc định
/// </summary>
public static class ProductDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as ProductService.Infrastructure.Data.Context.ProductDbContext;
        if (dbContext == null)
            return;

        // Seed Categories nếu chưa có
        if (!dbContext.Categories.Any())
        {
            var categories = new List<Category>
            {
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất phòng khách",
                    Description = "Các sản phẩm nội thất gỗ dành cho phòng khách",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất phòng ngủ",
                    Description = "Các sản phẩm nội thất gỗ dành cho phòng ngủ",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất phòng ăn",
                    Description = "Các sản phẩm nội thất gỗ dành cho phòng ăn",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất văn phòng",
                    Description = "Các sản phẩm nội thất gỗ dành cho văn phòng",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Đồ trang trí",
                    Description = "Các sản phẩm trang trí từ gỗ",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Đồ gỗ ngoài trời",
                    Description = "Các sản phẩm gỗ dùng ngoài trời",
                    ParentCategoryId = null,
                    Level = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            dbContext.Categories.AddRange(categories);
            await dbContext.SaveChangesAsync();
        }

        // Seed ProductMasters và ProductVersions nếu chưa có
        if (!dbContext.ProductMasters.Any())
        {
            // Các seller ID cần được lấy từ IdentityService
            // Sử dụng các GUIDs ngẫu nhiên cho demo
            var seller01Id = Guid.NewGuid();
            var seller02Id = Guid.NewGuid();

            var livingRoomCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng khách");
            var bedroomCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng ngủ");
            var diningCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng ăn");
            var officeCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất văn phòng");
            var decorCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Đồ trang trí");
            var outdoorCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Đồ gỗ ngoài trời");

            var productMasters = new List<ProductMaster>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = livingRoomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Ghế Sofa Gỗ Óc Chó Cao Cấp",
                    GlobalSku = "WOOD-SOFA-001",
                    Description = "Ghế sofa được làm từ gỗ óc chó tự nhiên, thiết kế hiện đại, phù hợp phòng khách rộng. Kích thước: 2m x 0.9m x 0.8m",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    PublishedAt = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = bedroomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Giường Ngủ Gỗ Sồi 1m8",
                    GlobalSku = "WOOD-BED-001",
                    Description = "Giường ngủ gỗ sồi Mỹ nhập khẩu, có ngăn kéo tiện lợi. Kích thước: 1.8m x 2m",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-45),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10),
                    PublishedAt = DateTime.UtcNow.AddDays(-10)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = diningCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Bàn Ăn Gỗ Teak 6 Ghế",
                    GlobalSku = "WOOD-TABLE-001",
                    Description = "Bộ bàn ăn 6 ghế gỗ teak cao cấp, thiết kế sang trọng. Kích thước bàn: 1.6m x 0.9m",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-15),
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15),
                    PublishedAt = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = officeCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Bàn Làm Việc Gỗ Ash Hiện Đại",
                    GlobalSku = "WOOD-DESK-001",
                    Description = "Bàn làm việc gỗ ash phong cách Bắc Âu, có ngăn kéo và giá sách. Kích thước: 1.4m x 0.6m",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-8),
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8),
                    PublishedAt = DateTime.UtcNow.AddDays(-8)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = decorCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Khung Gương Gỗ Hương Tròn",
                    GlobalSku = "WOOD-DECOR-001",
                    Description = "Khung gương trang trí được chạm khắc tinh xảo từ gỗ hương. Đường kính: 60cm",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-12),
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-12),
                    PublishedAt = DateTime.UtcNow.AddDays(-12)
                },
new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = outdoorCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Bàn Ghế Sân Vườn Gỗ Lim",
                    GlobalSku = null,
                    Description = "Bộ bàn ghế sân vườn 4 chỗ ngồi, gỗ lim chống mối mọt tự nhiên",
                    Status = ProductStatus.PENDING_APPROVAL,
                    ModerationStatus = ModerationStatus.PENDING,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = livingRoomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Kệ Tivi Gỗ Tần Bì",
                    GlobalSku = "WOOD-TV-001",
                    Description = "Kệ tivi gỗ tần bì thiết kế tối giản, nhiều ngăn lưu trữ. Kích thước: 1.8m x 0.4m x 0.5m",
                    Status = ProductStatus.APPROVED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = bedroomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Tủ Quần Áo Gỗ Sồi Trắng",
                    GlobalSku = null,
                    Description = "Tủ quần áo gỗ sồi trắng 4 cánh, thiết kế hiện đại với gương toàn thân",
                    Status = ProductStatus.REJECTED,
                    ModerationStatus = ModerationStatus.REJECTED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            dbContext.ProductMasters.AddRange(productMasters);
            await dbContext.SaveChangesAsync();

            // Seed ProductVersions cho các sản phẩm đã được tạo
            var sofa = productMasters.FirstOrDefault(p => p.Name == "Ghế Sofa Gỗ Óc Chó Cao Cấp");
            var bed = productMasters.FirstOrDefault(p => p.Name == "Giường Ngủ Gỗ Sồi 1m8");
            var table = productMasters.FirstOrDefault(p => p.Name == "Bàn Ăn Gỗ Teak 6 Ghế");
            var desk = productMasters.FirstOrDefault(p => p.Name == "Bàn Làm Việc Gỗ Ash Hiện Đại");
            var mirror = productMasters.FirstOrDefault(p => p.Name == "Khung Gương Gỗ Hương Tròn");
            var tvStand = productMasters.FirstOrDefault(p => p.Name == "Kệ Tivi Gỗ Tần Bì");

            var productVersions = new List<ProductVersion>();

            if (sofa != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = sofa.ProductId,
                    SellerSku = "SOFA-OC-CHO-V1",
                    VersionName = "Phiên bản tiêu chuẩn",
                    Price = 25000000, // 25,000,000 VND
                    StockQuantity = 10,
                    WeightGrams = 85000,
                    LengthCm = 200,
                    WidthCm = 90,
                    HeightCm = 80,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (bed != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = bed.ProductId,
                    SellerSku = "BED-SOI-1M8-V1",
                    VersionName = "Giường có ngăn kéo",
                    Price = 18000000, // 18,000,000 VND
                    StockQuantity = 15,
                    WeightGrams = 120000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (table != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = table.ProductId,
                    SellerSku = "TABLE-TEAK-6G-V1",
                    VersionName = "Combo bàn + 6 ghế",
                    Price = 32000000, // 32,000,000 VND
                    StockQuantity = 8,
                    WeightGrams = 95000,
                    LengthCm = 160,
                    WidthCm = 90,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (desk != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = desk.ProductId,
                    SellerSku = "DESK-ASH-MOD-V1",
                    VersionName = "Bàn có ngăn kéo và giá sách",
                    Price = 8500000, // 8,500,000 VND
                    StockQuantity = 20,
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 60,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (mirror != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = mirror.ProductId,
                    SellerSku = "MIRROR-HUONG-V1",
                    VersionName = "Khung gương tròn 60cm",
                    Price = 4500000, // 4,500,000 VND
                    StockQuantity = 25,
                    WeightGrams = 8000,
                    LengthCm = 60,
                    WidthCm = 60,
                    HeightCm = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (tvStand != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = tvStand.ProductId,
                    SellerSku = "TV-STAND-ASH-V1",
                    VersionName = "Kệ tivi 1m8",
                    Price = 9500000, // 9,500,000 VND
                    StockQuantity = 12,
                    WeightGrams = 45000,
                    LengthCm = 180,
                    WidthCm = 40,
                    HeightCm = 50,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (productVersions.Any())
            {
                dbContext.ProductVersions.AddRange(productVersions);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
