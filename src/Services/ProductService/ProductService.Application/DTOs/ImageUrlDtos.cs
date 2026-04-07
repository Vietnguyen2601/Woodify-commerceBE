namespace ProductService.Application.DTOs;

public class ImageUrlDto
{
    public Guid ImageId { get; set; }
    public string ImageType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public int SortOrder { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string? PublicId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// FE upload ảnh trực tiếp lên Cloudinary, sau đó gửi metadata xuống BE để lưu
/// </summary>
public class SaveImageUrlDto
{
    /// <summary>PRODUCT, PRODUCT_VERSION, REVIEW, AVATAR, SHOP_LOGO, SHOP_COVER, CATEGORY, BANNER, ADS</summary>
    public string ImageType { get; set; } = string.Empty;

    /// <summary>ID của entity tương ứng (ProductId, VersionId, ReviewId, ShopId,...)</summary>
    public Guid ReferenceId { get; set; }

    /// <summary>Secure URL từ Cloudinary</summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>Cloudinary public_id, dùng để xóa ảnh về sau</summary>
    public string? PublicId { get; set; }

    /// <summary>Thứ tự hiển thị. Nếu null, BE tự động tính (append vào cuối)</summary>
    public int? SortOrder { get; set; }
}

/// <summary>Lưu nhiều ảnh cùng lúc</summary>
public class BulkSaveImageUrlsDto
{
    public List<SaveImageUrlDto> Images { get; set; } = new();
}
