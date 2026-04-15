    namespace ProductService.Domain.Entities;

/// <summary>
/// Mirror tối thiểu thông tin shop để ProductService có thể hiển thị shopName mà không gọi HTTP.
/// Đồng bộ qua RabbitMQ (shop.created / shop.updated / shop.names.published).
/// </summary>
public class ShopRegistryEntry
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}

