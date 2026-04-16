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
            // Sử dụng các GUIDs từ Seller accounts đã được tạo
            var seller01Id = new Guid("f4a5b6c7-d8e9-4f0a-1b2c-3d4e5f6a7b8c"); // Shop của Seller 01
            var seller02Id = new Guid("a5b6c7d8-e9f0-4a1b-2c3d-4e5f6a7b8c9d"); // Shop của Seller 02

            var livingRoomCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng khách");
            var bedroomCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng ngủ");
            var diningCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất phòng ăn");
            var officeCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Nội thất văn phòng");
            var decorCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Đồ trang trí");
            var outdoorCat = dbContext.Categories.FirstOrDefault(c => c.Name == "Đồ gỗ ngoài trời");

            var productMasters = new List<ProductMaster>
            {
                // Seller 01 - Product 1
                new()
                {
                    ProductId = new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"),
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
                // Seller 01 - Product 2
                new()
                {
                    ProductId = new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"),
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
                // Seller 02 - Product 1
                new()
                {
                    ProductId = new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"),
                    ShopId = seller02Id,
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
                // Seller 02 - Product 2
                new()
                {
                    ProductId = new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"),
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
                }
            };

            dbContext.ProductMasters.AddRange(productMasters);
            await dbContext.SaveChangesAsync();

            // Seed ProductVersions: 2 versions cho mỗi product
            var sofa = productMasters.FirstOrDefault(p => p.ProductId == new Guid("b6c7d8e9-f0a1-4b2c-3d4e-5f6a7b8c9d0e"));
            var bed = productMasters.FirstOrDefault(p => p.ProductId == new Guid("c7d8e9f0-a1b2-4c3d-4e5f-6a7b8c9d0e1f"));
            var table = productMasters.FirstOrDefault(p => p.ProductId == new Guid("d8e9f0a1-b2c3-4d4e-5f6a-7b8c9d0e1f2a"));
            var desk = productMasters.FirstOrDefault(p => p.ProductId == new Guid("e9f0a1b2-c3d4-4e5f-6a7b-8c9d0e1f2a3b"));

            var productVersions = new List<ProductVersion>();

            // Sofa - Version 1 & 2
            if (sofa != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("f0a1b2c3-d4e5-4f6a-7b8c-9d0e1f2a3b4c"),
                    ProductId = sofa.ProductId,
                    SellerSku = "SOFA-OC-CHO-V1",
                    VersionNumber = 1,
                    VersionName = "Phiên bản tiêu chuẩn",
                    Price = 10000, // 25,000,000 VND
                    StockQuantity = 10,
                    WoodType = "Óc Chó",
                    WeightGrams = 85000,
                    LengthCm = 200,
                    WidthCm = 90,
                    HeightCm = 80,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25)
                });
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"),
                    ProductId = sofa.ProductId,
                    SellerSku = "SOFA-OC-CHO-V2",
                    VersionNumber = 2,
                    VersionName = "Phiên bản sang trọng",
                    Price = 10000, // 32,000,000 VND
                    StockQuantity = 5,
                    WoodType = "Óc Chó",
                    WeightGrams = 90000,
                    LengthCm = 200,
                    WidthCm = 100,
                    HeightCm = 85,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-20)
                });
            }

            // Bed - Version 1 & 2
            if (bed != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"),
                    ProductId = bed.ProductId,
                    SellerSku = "BED-SOI-1M8-V1",
                    VersionNumber = 1,
                    VersionName = "Giường có ngăn kéo",
                    Price = 18000, // 18,000,000 VND
                    StockQuantity = 15,
                    WoodType = "Sồi",
                    WeightGrams = 120000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-40),
                    UpdatedAt = DateTime.UtcNow.AddDays(-40)
                });
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"),
                    ProductId = bed.ProductId,
                    SellerSku = "BED-SOI-1M8-V2",
                    VersionNumber = 2,
                    VersionName = "Giường cao cấp ngoại nhập",
                    Price = 22000, // 22,000,000 VND
                    StockQuantity = 8,
                    WoodType = "Sồi",
                    WeightGrams = 125000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 45,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-35),
                    UpdatedAt = DateTime.UtcNow.AddDays(-35)
                });
            }

            // Table - Version 1 & 2
            if (table != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"),
                    ProductId = table.ProductId,
                    SellerSku = "TABLE-TEAK-6G-V1",
                    VersionNumber = 1,
                    VersionName = "Combo bàn + 6 ghế",
                    Price = 32000, // 32,000,000 VND
                    StockQuantity = 8,
                    WoodType = "Teak",
                    WeightGrams = 95000,
                    LengthCm = 160,
                    WidthCm = 90,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-55),
                    UpdatedAt = DateTime.UtcNow.AddDays(-55)
                });
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"),
                    ProductId = table.ProductId,
                    SellerSku = "TABLE-TEAK-6G-V2",
                    VersionNumber = 2,
                    VersionName = "Combo bàn ăn mở rộng",
                    Price = 38000, // 38,000,000 VND
                    StockQuantity = 5,
                    WoodType = "Teak",
                    WeightGrams = 100000,
                    LengthCm = 180,
                    WidthCm = 100,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-50),
                    UpdatedAt = DateTime.UtcNow.AddDays(-50)
                });
            }

            // Desk - Version 1 & 2
            if (desk != null)
            {
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"),
                    ProductId = desk.ProductId,
                    SellerSku = "DESK-ASH-MOD-V1",
                    VersionNumber = 1,
                    VersionName = "Bàn có ngăn kéo và giá sách",
                    Price = 85000, // 8,500,000 VND
                    StockQuantity = 20,
                    WoodType = "Ash",
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 60,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                });
                productVersions.Add(new ProductVersion
                {
                    VersionId = new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"),
                    ProductId = desk.ProductId,
                    SellerSku = "DESK-ASH-MOD-V2",
                    VersionNumber = 2,
                    VersionName = "Bàn làm việc Premium",
                    Price = 12000, // 12,000,000 VND
                    StockQuantity = 12,
                    WoodType = "Ash",
                    WeightGrams = 38000,
                    LengthCm = 160,
                    WidthCm = 70,
                    HeightCm = 75,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
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
