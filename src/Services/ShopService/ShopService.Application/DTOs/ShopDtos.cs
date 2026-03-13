using ShopService.Domain.Enums;

namespace ShopService.Application.DTOs;

public class ShopDto
{
    public Guid ShopId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateShopDto
{
    public Guid OwnerAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
}

public class UpdateShopDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
}

public class RegisterShopDto
{
    public Guid OwnerAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
}

public class RegisterShopResponseDto
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UpdateShopInfoDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? DefaultPickupAddress { get; set; }
    public Guid? DefaultProvider { get; set; }
}

public class UpdateShopInfoResponseDto
{
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? DefaultProvider { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UpdateShopStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
}

public class UpdateShopStatusResponseDto
{
    public Guid ShopId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
