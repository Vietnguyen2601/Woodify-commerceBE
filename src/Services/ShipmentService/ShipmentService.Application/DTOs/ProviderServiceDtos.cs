namespace ShipmentService.Application.DTOs;

// ── ProviderService ───────────────────────────────────────────────────────────

public class CreateProviderServiceDto
{
    public Guid ProviderId { get; set; }
    public string Code { get; set; } = string.Empty;   // ECO, STD, EXP, SUP
    public string Name { get; set; } = string.Empty;   // Tiết kiệm, Nhanh, Hỏa tốc
    public string? SpeedLevel { get; set; }             // ECONOMY, STANDARD, EXPRESS
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public double? MultiplierFee { get; set; }
}

public class UpdateProviderServiceDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? SpeedLevel { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public bool? IsActive { get; set; }
    public double? MultiplierFee { get; set; }
}

public class ProviderServiceDto
{
    public Guid ServiceId { get; set; }
    public Guid ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SpeedLevel { get; set; }
    public int? EstimatedDaysMin { get; set; }
    public int? EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; }
    public double? MultiplierFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
