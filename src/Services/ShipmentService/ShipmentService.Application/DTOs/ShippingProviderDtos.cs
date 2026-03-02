namespace ShipmentService.Application.DTOs;

// ── ShippingProvider ──────────────────────────────────────────────────────────

public class CreateShippingProviderDto
{
    public string Name { get; set; } = string.Empty;
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateShippingProviderDto
{
    public string? Name { get; set; }
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool? IsActive { get; set; }
}

public class ShippingProviderDto
{
    public Guid ProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SupportPhone { get; set; }
    public string? SupportEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
