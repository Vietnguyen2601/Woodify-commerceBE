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
                    Name = "Ghế Sofa Gỗ Óc Chó Cao Cấp",
                    GlobalSku = "WOOD-SOFA-001",
                    ImgUrl = "https://example.com/images/sofa-oc-cho.jpg",
                    Description = "Ghế sofa được làm từ gỗ óc chó tự nhiên, thiết kế hiện đại, phù hợp phòng khách rộng. Kích thước: 2m x 0.9m x 0.8m",
                    ArAvailable = true,
                    ArModelUrl = "https://example.com/ar/sofa-oc-cho.glb",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-5),
                    AvgRating = 4.8m,
                    ReviewCount = 25,
                    SoldCount = 150,
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
                    ImgUrl = "https://example.com/images/giuong-soi.jpg",
                    Description = "Giường ngủ gỗ sồi Mỹ nhập khẩu, có ngăn kéo tiện lợi. Kích thước: 1.8m x 2m",
                    ArAvailable = true,
                    ArModelUrl = "https://example.com/ar/giuong-soi.glb",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-10),
                    AvgRating = 4.5m,
                    ReviewCount = 18,
                    SoldCount = 85,
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
                    ImgUrl = "https://example.com/images/ban-an-teak.jpg",
                    Description = "Bộ bàn ăn 6 ghế gỗ teak cao cấp, thiết kế sang trọng. Kích thước bàn: 1.6m x 0.9m",
                    ArAvailable = false,
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-15),
                    AvgRating = 4.7m,
                    ReviewCount = 42,
                    SoldCount = 120,
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
                    ImgUrl = "https://example.com/images/ban-lam-viec-ash.jpg",
                    Description = "Bàn làm việc gỗ ash phong cách Bắc Âu, có ngăn kéo và giá sách. Kích thước: 1.4m x 0.6m",
                    ArAvailable = true,
                    ArModelUrl = "https://example.com/ar/ban-ash.glb",
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-8),
                    AvgRating = 4.6m,
                    ReviewCount = 33,
                    SoldCount = 95,
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
                    ImgUrl = "https://example.com/images/khung-guong.jpg",
                    Description = "Khung gương trang trí được chạm khắc tinh xảo từ gỗ hương. Đường kính: 60cm",
                    ArAvailable = false,
                    Status = ProductStatus.PUBLISHED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-12),
                    AvgRating = 4.0m,
                    ReviewCount = 12,
                    SoldCount = 45,
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
                    GlobalSku = null, // Chưa có version nên chưa có GlobalSku
                    ImgUrl = "https://example.com/images/ban-ghe-san-vuon.jpg",
                    Description = "Bộ bàn ghế sân vườn 4 chỗ ngồi, gỗ lim chống mối mọt tự nhiên",
                    ArAvailable = false,
                    Status = ProductStatus.PENDING_APPROVAL,
                    ModerationStatus = ModerationStatus.PENDING,
                    AvgRating = 0,
                    ReviewCount = 0,
                    SoldCount = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller01Id,
                    CategoryId = livingRoomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Kệ Tivi Gỗ Tần Bì",
                    GlobalSku = "WOOD-TV-001", // Đã có version nên có GlobalSku
                    ImgUrl = "https://example.com/images/ke-tivi.jpg",
                    Description = "Kệ tivi gỗ tần bì thiết kế tối giản, nhiều ngăn lưu trữ. Kích thước: 1.8m x 0.4m x 0.5m",
                    ArAvailable = true,
                    ArModelUrl = "https://example.com/ar/ke-tivi.glb",
                    Status = ProductStatus.APPROVED,
                    ModerationStatus = ModerationStatus.APPROVED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-1),
                    AvgRating = 0,
                    ReviewCount = 0,
                    SoldCount = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ShopId = seller02Id,
                    CategoryId = bedroomCat?.CategoryId ?? Guid.NewGuid(),
                    Name = "Tủ Quần Áo Gỗ Sồi Trắng",
                    GlobalSku = null, // Bị reject nên chưa tạo version, không có GlobalSku
                    ImgUrl = "https://example.com/images/tu-quan-ao.jpg",
                    Description = "Tủ quần áo gỗ sồi trắng 4 cánh, thiết kế hiện đại với gương toàn thân",
                    ArAvailable = false,
                    Status = ProductStatus.REJECTED,
                    ModerationStatus = ModerationStatus.REJECTED,
                    ModeratedAt = DateTime.UtcNow.AddDays(-1),
                    RejectionReason = "Hình ảnh sản phẩm không rõ ràng, vui lòng cung cấp hình ảnh chất lượng tốt hơn",
                    ModerationNotes = "Cần cập nhật ảnh chụp từ nhiều góc độ khác nhau",
                    AvgRating = 0,
                    ReviewCount = 0,
                    SoldCount = 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            dbContext.ProductMasters.AddRange(productMasters);
            await dbContext.SaveChangesAsync();

            // TODO: Seed ProductVersions nếu cần
            // Hiện tại tạm thời không seed ProductVersion do foreign key constraint
            // Có thể tạo ProductVersion qua API sau khi service đã chạy
            
            /*
            // Seed ProductVersions
            // Chỉ seed version cho các sản phẩm đã có GlobalSku (đã được tạo version trước đó)
            var sofa = productMasters.FirstOrDefault(p => p.Name == "Ghế Sofa Gỗ Óc Chó Cao Cấp");
            var bed = productMasters.FirstOrDefault(p => p.Name == "Giường Ngủ Gỗ Sồi 1m8");
            var table = productMasters.FirstOrDefault(p => p.Name == "Bàn Ăn Gỗ Teak 6 Ghế");
            var desk = productMasters.FirstOrDefault(p => p.Name == "Bàn Làm Việc Gỗ Ash Hiện Đại");
            var mirror = productMasters.FirstOrDefault(p => p.Name == "Khung Gương Gỗ Hương Tròn");
            var tvStand = productMasters.FirstOrDefault(p => p.Name == "Kệ Tivi Gỗ Tần Bì");

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
                    VersionId = tvStand?.ProductId ?? Guid.NewGuid(),
                    Title = "Kệ Tivi Gỗ Tần Bì",
                    Description = "Kệ tivi gỗ tần bì thiết kế tối giản, nhiều ngăn lưu trữ. Kích thước: 1.8m x 0.4m x 0.5m",
                    PriceCents = 950000000,
                    Currency = "VND",
                    Sku = "TV-STAND-ASH-V1",
                    ArAvailable = true,
                    IsDeleted = false,
                    CreatedBy = seller01Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
                // Lưu ý: Không tạo version cho "Bàn Ghế Sân Vườn" và "Tủ Quần Áo" 
                // vì chúng chưa có GlobalSku (chưa được tạo version)
            };

            dbContext.ProductVersions.AddRange(productVersions);
            await dbContext.SaveChangesAsync();
            */
        }
    }
}
