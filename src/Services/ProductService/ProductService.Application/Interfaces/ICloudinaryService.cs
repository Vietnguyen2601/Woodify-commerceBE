using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Interfaces;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadAsync(IFormFile file, string folder = "woodify");
}

public class CloudinaryUploadResult
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}
