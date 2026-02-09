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
                    GlobalSku = "WOOD-SOFA-001",
                    Status = ProductStatus.PUBLISHED,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = bedroomCat?.CategoryId ?? Guid.NewGuid(),
                    GlobalSku = "WOOD-BED-001",
                    Status = ProductStatus.PUBLISHED,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = diningCat?.CategoryId ?? Guid.NewGuid(),
                    GlobalSku = "WOOD-TABLE-001",
                    Status = ProductStatus.PUBLISHED,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = officeCat?.CategoryId ?? Guid.NewGuid(),
                    GlobalSku = "WOOD-DESK-001",
                    Status = ProductStatus.PUBLISHED,
                    Certified = true,
                    CurrentVersionId = null,
                    AvgRating = 4.6m,
                    ReviewCount = 33,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = decorCat?.CategoryId ?? Guid.NewGuid(),
                    GlobalSku = "WOOD-DECOR-001",
                    Status = ProductStatus.PUBLISHED,
                    Certified = false,
                    CurrentVersionId = null,
                    AvgRating = 4.0m,
                    ReviewCount = 12,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = outdoorCat?.CategoryId ?? Guid.NewGuid(),
                    GlobalSku = "WOOD-OUTDOOR-001",
                    Status = ProductStatus.DRAFT,
                    Certified = true,
                    CurrentVersionId = null,
                    AvgRating = 0,
                    ReviewCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            dbContext.ProductMasters.AddRange(productMasters);
            await dbContext.SaveChangesAsync();

            // Seed ProductVersions
            var sofa = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-SOFA-001");
            var bed = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-BED-001");
            var table = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-TABLE-001");
            var desk = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-DESK-001");
            var mirror = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-DECOR-001");
            var outdoor = productMasters.FirstOrDefault(p => p.GlobalSku == "WOOD-OUTDOOR-001");

            var productVersions = new List<ProductVersion>
            {
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = sofa?.ProductId ?? Guid.NewGuid(),
                    Title = "Ghế Sofa Gỗ Óc Chó Cao Cấp",
                    Description = "Ghế sofa được làm từ gỗ óc chó tự nhiên, thiết kế hiện đại, phù hợp phòng khách rộng. Kích thước: 2m x 0.9m x 0.8m",
                    PriceCents = 2500000000,
                    Currency = "VND",
                    Sku = "SOFA-OC-CHO-V1",
                    ArAvailable = true,
                    IsDeleted = false,
                    CreatedBy = seller01Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = bed?.ProductId ?? Guid.NewGuid(),
                    Title = "Giường Ngủ Gỗ Sồi 1m8",
                    Description = "Giường ngủ gỗ sồi Mỹ nhập khẩu, có ngăn kéo tiện lợi. Kích thước: 1.8m x 2m",
                    PriceCents = 1800000000,
                    Currency = "VND",
                    Sku = "BED-SOI-1M8-V1",
                    ArAvailable = true,
                    IsDeleted = false,
                    CreatedBy = seller01Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = table?.ProductId ?? Guid.NewGuid(),
                    Title = "Bàn Ăn Gỗ Teak 6 Ghế",
                    Description = "Bộ bàn ăn 6 ghế gỗ teak cao cấp, thiết kế sang trọng. Kích thước bàn: 1.6m x 0.9m",
                    PriceCents = 3200000000,
                    Currency = "VND",
                    Sku = "TABLE-TEAK-6G-V1",
                    ArAvailable = false,
                    IsDeleted = false,
                    CreatedBy = seller01Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = desk?.ProductId ?? Guid.NewGuid(),
                    Title = "Bàn Làm Việc Gỗ Ash Hiện Đại",
                    Description = "Bàn làm việc gỗ ash phong cách Bắc Âu, có ngăn kéo và giá sách. Kích thước: 1.4m x 0.6m",
                    PriceCents = 850000000,
                    Currency = "VND",
                    Sku = "DESK-ASH-MOD-V1",
                    ArAvailable = true,
                    IsDeleted = false,
                    CreatedBy = seller02Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = mirror?.ProductId ?? Guid.NewGuid(),
                    Title = "Khung Gương Gỗ Hương Tròn",
                    Description = "Khung gương trang trí được chạm khắc tinh xảo từ gỗ hương. Đường kính: 60cm",
                    PriceCents = 450000000,
                    Currency = "VND",
                    Sku = "MIRROR-HUONG-V1",
                    ArAvailable = false,
                    IsDeleted = false,
                    CreatedBy = seller02Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    VersionId = Guid.NewGuid(),
                    ProductId = outdoor?.ProductId ?? Guid.NewGuid(),
                    Title = "Bàn Ghế Sân Vườn Gỗ Lim",
                    Description = "Bộ bàn ghế sân vườn 4 chỗ ngồi, gỗ lim chống mối mọt tự nhiên",
                    PriceCents = 1500000000,
                    Currency = "VND",
                    Sku = "OUTDOOR-LIM-4G-V1",
                    ArAvailable = false,
                    IsDeleted = false,
                    CreatedBy = seller02Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            dbContext.ProductVersions.AddRange(productVersions);
            await dbContext.SaveChangesAsync();
        }
    }
}
