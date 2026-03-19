using ProductService.Application.DTOs;
using Shared.Results;

namespace ProductService.Application.Interfaces;

public interface IImageUrlService
{
    /// <summary>FE gửi metadata sau khi upload lên Cloudinary — lưu 1 ảnh</summary>
    Task<ServiceResult<ImageUrlDto>> SaveImageAsync(SaveImageUrlDto dto);

    /// <summary>Lưu nhiều ảnh cùng lúc</summary>
    Task<ServiceResult<List<ImageUrlDto>>> SaveImagesAsync(BulkSaveImageUrlsDto dto);

    /// <summary>Lấy danh sách ảnh, sắp xếp theo sortOrder ASC</summary>
    Task<ServiceResult<List<ImageUrlDto>>> GetImagesAsync(string imageType, Guid referenceId);

    /// <summary>Lấy ảnh chính (sortOrder nhỏ nhất)</summary>
    Task<ServiceResult<ImageUrlDto>> GetPrimaryImageAsync(string imageType, Guid referenceId);
}
