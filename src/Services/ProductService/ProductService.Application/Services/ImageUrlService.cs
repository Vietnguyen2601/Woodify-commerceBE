using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Repositories.IRepositories;
using Shared.Results;
using Shared.Events;

namespace ProductService.Application.Services;

public class ImageUrlService : IImageUrlService
{
    private readonly IImageUrlRepository _imageUrlRepository;
    private readonly ProductEventPublisher _eventPublisher;

    private static readonly HashSet<string> ValidImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PRODUCT", "PRODUCT_VERSION", "REVIEW", "AVATAR",
        "SHOP_LOGO", "SHOP_COVER", "CATEGORY", "BANNER", "ADS"
    };

    public ImageUrlService(IImageUrlRepository imageUrlRepository, ProductEventPublisher eventPublisher)
    {
        _imageUrlRepository = imageUrlRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ServiceResult<ImageUrlDto>> SaveImageAsync(SaveImageUrlDto dto)
    {
        if (!ValidImageTypes.Contains(dto.ImageType))
            return ServiceResult<ImageUrlDto>.BadRequest(
                $"Invalid imageType '{dto.ImageType}'. Allowed: {string.Join(", ", ValidImageTypes)}");

        if (string.IsNullOrWhiteSpace(dto.OriginalUrl))
            return ServiceResult<ImageUrlDto>.BadRequest("OriginalUrl is required");

        if (dto.ReferenceId == Guid.Empty)
            return ServiceResult<ImageUrlDto>.BadRequest("ReferenceId is required");

        var sortOrder = dto.SortOrder ??
            await _imageUrlRepository.GetNextSortOrderAsync(dto.ImageType.ToUpper(), dto.ReferenceId);

        var entity = new ImageUrl
        {
            ImageId = Guid.NewGuid(),
            ImageType = dto.ImageType.ToUpper(),
            ReferenceId = dto.ReferenceId,
            SortOrder = sortOrder,
            OriginalUrl = dto.OriginalUrl,
            PublicId = dto.PublicId,
            CreatedAt = DateTime.UtcNow
        };

        await _imageUrlRepository.CreateAsync(entity);

        // Publish event khi ảnh chính (sort_order = 0) của ProductVersion được upload
        if (entity.ImageType == "PRODUCT_VERSION" && entity.SortOrder == 0)
        {
            _eventPublisher.PublishImageUrlUpdated(new ImageUrlUpdatedEvent
            {
                VersionId = entity.ReferenceId,
                ThumbnailUrl = entity.OriginalUrl,
                UpdatedAt = DateTime.UtcNow,
                EventType = "ImageUrlUpdated"
            });
        }

        return ServiceResult<ImageUrlDto>.Created(ToDto(entity), "Image saved successfully");
    }

    public async Task<ServiceResult<List<ImageUrlDto>>> SaveImagesAsync(BulkSaveImageUrlsDto dto)
    {
        if (dto.Images == null || dto.Images.Count == 0)
            return ServiceResult<List<ImageUrlDto>>.BadRequest("No images provided");

        var entities = new List<ImageUrl>();

        foreach (var image in dto.Images)
        {
            if (!ValidImageTypes.Contains(image.ImageType))
                return ServiceResult<List<ImageUrlDto>>.BadRequest(
                    $"Invalid imageType '{image.ImageType}'");

            if (string.IsNullOrWhiteSpace(image.OriginalUrl))
                return ServiceResult<List<ImageUrlDto>>.BadRequest(
                    $"OriginalUrl is required for all images");

            var sortOrder = image.SortOrder ??
                await _imageUrlRepository.GetNextSortOrderAsync(image.ImageType.ToUpper(), image.ReferenceId);

            entities.Add(new ImageUrl
            {
                ImageId = Guid.NewGuid(),
                ImageType = image.ImageType.ToUpper(),
                ReferenceId = image.ReferenceId,
                SortOrder = sortOrder,
                OriginalUrl = image.OriginalUrl,
                PublicId = image.PublicId,
                CreatedAt = DateTime.UtcNow
            });
        }

        var saved = await _imageUrlRepository.BulkCreateAsync(entities);

        // Publish events cho ảnh chính (sort_order = 0) của ProductVersion
        foreach (var image in saved)
        {
            if (image.ImageType == "PRODUCT_VERSION" && image.SortOrder == 0)
            {
                _eventPublisher.PublishImageUrlUpdated(new ImageUrlUpdatedEvent
                {
                    VersionId = image.ReferenceId,
                    ThumbnailUrl = image.OriginalUrl,
                    UpdatedAt = DateTime.UtcNow,
                    EventType = "ImageUrlUpdated"
                });
            }
        }

        return ServiceResult<List<ImageUrlDto>>.Created(saved.Select(ToDto).ToList(),
            $"{saved.Count} image(s) saved successfully");
    }

    public async Task<ServiceResult<List<ImageUrlDto>>> GetImagesAsync(string imageType, Guid referenceId)
    {
        var images = await _imageUrlRepository.GetByTypeAndReferenceAsync(imageType, referenceId);
        return ServiceResult<List<ImageUrlDto>>.Success(images.Select(ToDto).ToList());
    }

    public async Task<ServiceResult<ImageUrlDto>> GetPrimaryImageAsync(string imageType, Guid referenceId)
    {
        var image = await _imageUrlRepository.GetPrimaryImageAsync(imageType, referenceId);
        if (image == null)
            return ServiceResult<ImageUrlDto>.NotFound("No image found for this reference");

        return ServiceResult<ImageUrlDto>.Success(ToDto(image));
    }

    public async Task<ServiceResult<List<ImageUrlDto>>> GetAllByTypeAsync(string imageType)
    {
        if (!ValidImageTypes.Contains(imageType))
            return ServiceResult<List<ImageUrlDto>>.BadRequest(
                $"Invalid imageType '{imageType}'. Allowed: {string.Join(", ", ValidImageTypes)}");

        var images = await _imageUrlRepository.GetAllByTypeAsync(imageType.ToUpper());
        return ServiceResult<List<ImageUrlDto>>.Success(images.Select(ToDto).ToList());
    }

    public async Task<ServiceResult> DeleteImageAsync(Guid imageId)
    {
        var deleted = await _imageUrlRepository.DeleteByIdAsync(imageId);
        if (!deleted)
            return ServiceResult.NotFound("Image not found");

        return ServiceResult.Success("Image deleted successfully");
    }

    private static ImageUrlDto ToDto(ImageUrl img) => new()
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
