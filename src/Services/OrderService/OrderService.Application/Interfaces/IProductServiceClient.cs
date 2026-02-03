using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface để gọi ProductService
/// </summary>
public interface IProductServiceClient
{
    Task<ProductVersionDto?> GetProductVersionAsync(Guid versionId);
    Task<bool> IsProductPublishedAsync(Guid productId);
}
