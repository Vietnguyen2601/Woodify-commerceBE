using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappers;

public static class ImageUrlMapper
{
    public static ImageUrlDto ToDto(this ImageUrl img) => new()
    {
        ImageId = img.ImageId,
        ImageType = img.ImageType,
        ReferenceId = img.ReferenceId,
        SortOrder = img.SortOrder,
        OriginalUrl = img.OriginalUrl,
        PublicId = img.PublicId,
        CreatedAt = img.CreatedAt
    };
}
