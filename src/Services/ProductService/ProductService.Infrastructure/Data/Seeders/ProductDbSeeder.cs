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
                    ImageUrl = "https://example.com/images/categories/phong-khach.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất phòng ngủ",
                    Description = "Các sản phẩm nội thất gỗ dành cho phòng ngủ",
                    ImageUrl = "https://example.com/images/categories/phong-ngu.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất phòng ăn",
                    Description = "Các sản phẩm nội thất gỗ dành cho phòng ăn",
                    ImageUrl = "https://example.com/images/categories/phong-an.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nội thất văn phòng",
                    Description = "Các sản phẩm nội thất gỗ dành cho văn phòng",
                    ImageUrl = "https://example.com/images/categories/van-phong.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Đồ trang trí",
                    Description = "Các sản phẩm trang trí từ gỗ",
                    ImageUrl = "https://example.com/images/categories/trang-tri.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Đồ gỗ ngoài trời",
                    Description = "Các sản phẩm gỗ dùng ngoài trời",
                    ImageUrl = "https://example.com/images/categories/ngoai-troi.jpg",
                    ParentCategoryId = null,
                    Level = 0,
                    DisplayOrder = 6,
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
                    VersionNumber = 1,
                    VersionName = "Phiên bản tiêu chuẩn",
                    PriceCents = 2500000000, // 25,000,000 VND
                    BasePriceCents = 2800000000,
                    StockQuantity = 10,
                    LowStockThreshold = 5,
                    AllowBackorder = false,
                    WeightGrams = 85000,
                    LengthCm = 200,
                    WidthCm = 90,
                    HeightCm = 80,
                    VolumeCm3 = 1440000,
                    BulkyType = "BULKY",
                    IsFragile = false,
                    RequiresSpecialHandling = true,
                    WarrantyMonths = 24,
                    WarrantyTerms = "Bảo hành 24 tháng lỗi do nhà sản xuất",
                    IsBundle = false,
                    BundleDiscountCents = 0,
                    PrimaryImageUrl = "https://example.com/images/sofa-oc-cho-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
                    VersionNumber = 1,
                    VersionName = "Giường có ngăn kéo",
                    PriceCents = 1800000000, // 18,000,000 VND
                    BasePriceCents = 2000000000,
                    StockQuantity = 15,
                    LowStockThreshold = 3,
                    AllowBackorder = true,
                    WeightGrams = 120000,
                    LengthCm = 200,
                    WidthCm = 180,
                    HeightCm = 40,
                    VolumeCm3 = 1440000,
                    BulkyType = "SUPER_BULKY",
                    IsFragile = false,
                    RequiresSpecialHandling = true,
                    WarrantyMonths = 36,
                    WarrantyTerms = "Bảo hành 36 tháng, miễn phí bảo trì định kỳ",
                    IsBundle = false,
                    BundleDiscountCents = 0,
                    PrimaryImageUrl = "https://example.com/images/bed-soi-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
                    VersionNumber = 1,
                    VersionName = "Combo bàn + 6 ghế",
                    PriceCents = 3200000000, // 32,000,000 VND
                    BasePriceCents = 4000000000,
                    StockQuantity = 8,
                    LowStockThreshold = 2,
                    AllowBackorder = false,
                    WeightGrams = 95000,
                    LengthCm = 160,
                    WidthCm = 90,
                    HeightCm = 75,
                    VolumeCm3 = 1080000,
                    BulkyType = "BULKY",
                    IsFragile = false,
                    RequiresSpecialHandling = true,
                    WarrantyMonths = 24,
                    WarrantyTerms = "Bảo hành 24 tháng toàn bộ bộ sản phẩm",
                    IsBundle = true,
                    BundleDiscountCents = 800000000, // Giảm 8 triệu khi mua combo
                    PrimaryImageUrl = "https://example.com/images/table-teak-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
                    VersionNumber = 1,
                    VersionName = "Bàn có ngăn kéo và giá sách",
                    PriceCents = 850000000, // 8,500,000 VND
                    BasePriceCents = 950000000,
                    StockQuantity = 20,
                    LowStockThreshold = 5,
                    AllowBackorder = true,
                    WeightGrams = 35000,
                    LengthCm = 140,
                    WidthCm = 60,
                    HeightCm = 75,
                    VolumeCm3 = 630000,
                    BulkyType = "NORMAL",
                    IsFragile = false,
                    RequiresSpecialHandling = false,
                    WarrantyMonths = 18,
                    WarrantyTerms = "Bảo hành 18 tháng",
                    IsBundle = false,
                    BundleDiscountCents = 0,
                    PrimaryImageUrl = "https://example.com/images/desk-ash-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
                    VersionNumber = 1,
                    VersionName = "Khung gương tròn 60cm",
                    PriceCents = 450000000, // 4,500,000 VND
                    StockQuantity = 25,
                    LowStockThreshold = 5,
                    AllowBackorder = true,
                    WeightGrams = 8000,
                    LengthCm = 60,
                    WidthCm = 60,
                    HeightCm = 5,
                    VolumeCm3 = 18000,
                    BulkyType = "NORMAL",
                    IsFragile = true,
                    RequiresSpecialHandling = true,
                    WarrantyMonths = 12,
                    WarrantyTerms = "Bảo hành 12 tháng khung gỗ",
                    IsBundle = false,
                    BundleDiscountCents = 0,
                    PrimaryImageUrl = "https://example.com/images/mirror-huong-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
                    VersionNumber = 1,
                    VersionName = "Kệ tivi 1m8",
                    PriceCents = 950000000, // 9,500,000 VND
                    BasePriceCents = 1100000000,
                    StockQuantity = 12,
                    LowStockThreshold = 3,
                    AllowBackorder = false,
                    WeightGrams = 45000,
                    LengthCm = 180,
                    WidthCm = 40,
                    HeightCm = 50,
                    VolumeCm3 = 360000,
                    BulkyType = "NORMAL",
                    IsFragile = false,
                    RequiresSpecialHandling = false,
                    WarrantyMonths = 24,
                    WarrantyTerms = "Bảo hành 24 tháng",
                    IsBundle = false,
                    BundleDiscountCents = 0,
                    PrimaryImageUrl = "https://example.com/images/tv-stand-ash-v1.jpg",
                    IsActive = true,
                    IsDefault = true,
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
