namespace ProductService.Domain.Entities;

/// <summary>
/// Entity ImageUrl - Bảng image_urls
/// Lưu trữ đường dẫn ảnh Cloudinary cho các entity trong hệ thống
/// </summary>
public class ImageUrl
{
    public Guid ImageId { get; set; } = Guid.NewGuid();

    /// <summary>Loại entity: PRODUCT, PRODUCT_VERSION, REVIEW, AVATAR, SHOP_LOGO, SHOP_COVER, CATEGORY, BANNER, ADS</summary>
    public string ImageType { get; set; } = string.Empty;

    /// <summary>ID của entity tương ứng (ProductId, VersionId, ReviewId, ...)</summary>
    public Guid ReferenceId { get; set; }

    /// <summary>Thứ tự hiển thị. Ảnh có SortOrder = 0 là ảnh chính.</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>URL Cloudinary (secure https)</summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>Cloudinary public_id, dùng để xóa ảnh sau này</summary>
    public string? PublicId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
