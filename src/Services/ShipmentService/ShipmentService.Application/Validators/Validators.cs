using FluentValidation;
using ShipmentService.Application.DTOs;

namespace ShipmentService.Application.Validators;

// ── CreateShipmentValidator ───────────────────────────────────────────────────

public class CreateShipmentValidator : AbstractValidator<CreateShipmentDto>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.ProviderServiceCode)
            .NotEmpty().WithMessage("ProviderServiceCode is required");
    }
}

// ── UpdateShipmentValidator ───────────────────────────────────────────────────

public class UpdateShipmentValidator : AbstractValidator<UpdateShipmentDto>
{
    private static readonly string[] AllowedBulkyTypes = { "NORMAL", "BULKY", "SUPER_BULKY" };

    public UpdateShipmentValidator()
    {
        RuleFor(x => x.TotalWeightGrams)
            .GreaterThan(0).WithMessage("TotalWeightGrams must be greater than 0")
            .When(x => x.TotalWeightGrams.HasValue);

        RuleFor(x => x.FinalShippingFeeCents)
            .GreaterThanOrEqualTo(0).WithMessage("FinalShippingFeeCents must be >= 0")
            .When(x => x.FinalShippingFeeCents.HasValue);

        RuleFor(x => x.BulkyType)
            .Must(b => b == null || AllowedBulkyTypes.Contains(b))
            .WithMessage($"BulkyType must be one of: {string.Join(", ", AllowedBulkyTypes)}")
            .When(x => x.BulkyType != null);
    }
}

// ── UpdateShipmentStatusValidator ─────────────────────────────────────────────

public class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusDto>
{
    private static readonly string[] AllowedStatuses =
    {
        "DRAFT", "PENDING", "PICKUP_SCHEDULED", "PICKED_UP", "IN_TRANSIT",
        "OUT_FOR_DELIVERY", "DELIVERED", "DELIVERY_FAILED",
        "RETURNING", "RETURNED", "CANCELLED"
    };

    public UpdateShipmentStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}

// ── CreateShippingProviderValidator ──────────────────────────────────────────

public class CreateShippingProviderValidator : AbstractValidator<CreateShippingProviderDto>
{
    public CreateShippingProviderValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.SupportEmail)
            .EmailAddress().WithMessage("SupportEmail is not valid")
            .When(x => !string.IsNullOrEmpty(x.SupportEmail));
    }
}

// ── UpdateShippingProviderValidator ──────────────────────────────────────────

public class UpdateShippingProviderValidator : AbstractValidator<UpdateShippingProviderDto>
{
    public UpdateShippingProviderValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.SupportEmail)
            .EmailAddress().WithMessage("SupportEmail is not valid")
            .When(x => !string.IsNullOrEmpty(x.SupportEmail));
    }
}

// ── CreateProviderServiceValidator ───────────────────────────────────────────

public class CreateProviderServiceValidator : AbstractValidator<CreateProviderServiceDto>
{
    private static readonly string[] AllowedSpeedLevels = { "ECONOMY", "STANDARD", "EXPRESS", "SUPER_EXPRESS" };

    public CreateProviderServiceValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("ProviderId is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(20).WithMessage("Code cannot exceed 20 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.SpeedLevel)
            .Must(s => s == null || AllowedSpeedLevels.Contains(s))
            .WithMessage($"SpeedLevel must be one of: {string.Join(", ", AllowedSpeedLevels)}")
            .When(x => x.SpeedLevel != null);

        RuleFor(x => x.EstimatedDaysMin)
            .GreaterThanOrEqualTo(0).WithMessage("EstimatedDaysMin must be >= 0")
            .When(x => x.EstimatedDaysMin.HasValue);

        RuleFor(x => x.EstimatedDaysMax)
            .GreaterThanOrEqualTo(0).WithMessage("EstimatedDaysMax must be >= 0")
            .When(x => x.EstimatedDaysMax.HasValue);

        RuleFor(x => x.MultiplierFee)
            .GreaterThan(0).WithMessage("MultiplierFee must be > 0")
            .When(x => x.MultiplierFee.HasValue);
    }
}

// ── ShippingFeePreviewValidator ───────────────────────────────────────────────

public class ShippingFeePreviewValidator : AbstractValidator<ShippingFeePreviewRequest>
{
    private static readonly string[] AllowedBulkyTypes = { "NORMAL", "BULKY", "SUPER_BULKY" };

    public ShippingFeePreviewValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("shop_id là bắt buộc.");

        RuleFor(x => x.ProviderServiceCode)
            .NotEmpty().WithMessage("provider_service_code là bắt buộc.")
            .MaximumLength(20).WithMessage("provider_service_code tối đa 20 ký tự.");

        RuleFor(x => x.TotalWeightGrams)
            .GreaterThan(0).WithMessage("total_weight_grams phải lớn hơn 0.");

        RuleFor(x => x.BulkyType)
            .NotEmpty().WithMessage("bulky_type là bắt buộc.")
            .Must(b => AllowedBulkyTypes.Contains(b?.ToUpperInvariant()))
            .WithMessage($"bulky_type phải là: {string.Join(", ", AllowedBulkyTypes)}.");

        RuleFor(x => x.PickupAddressId)
            .NotEmpty().WithMessage("pickup_address_id là bắt buộc.");

        RuleFor(x => x.DeliveryAddressId)
            .NotEmpty().WithMessage("delivery_address_id là bắt buộc.");

        RuleFor(x => x.SubtotalCents)
            .GreaterThanOrEqualTo(0).WithMessage("subtotal_cents phải >= 0.")
            .When(x => x.SubtotalCents.HasValue);
    }
}
