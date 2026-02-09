namespace OrderService.Application.DTOs;

/// <summary>
/// DTO để nhận thông tin product version từ ProductService
/// </summary>
public class ProductVersionDto
{
    public Guid VersionId { get; set; }
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? PriceCents { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Sku { get; set; }
    public string? ProductStatus { get; set; }
}

/// <summary>
/// Response wrapper từ ProductService
/// </summary>
public class ProductServiceResponse<T>
{
    public int Status { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}
