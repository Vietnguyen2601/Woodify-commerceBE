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

// ── Paged list ───────────────────────────────────────────────────────────────

public class GetProvidersQueryDto
{
    public int Page { get; set; } = 1;

    public int Limit { get; set; } = 20;
}

public class PaginationResultDto
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
}

public class ShippingProviderPagedDto
{
    public List<ShippingProviderDto> Providers { get; set; } = [];
    public PaginationResultDto Pagination { get; set; } = new();
}
