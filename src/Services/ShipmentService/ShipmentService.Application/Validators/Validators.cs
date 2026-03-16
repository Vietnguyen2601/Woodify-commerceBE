using FluentValidation;
using ShipmentService.Application.DTOs;

namespace ShipmentService.Application.Validators;

public class CreateShipmentValidator : AbstractValidator<CreateShipmentDto>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("ShopId is required");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
    }
}

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

public class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusDto>
{
    private static readonly string[] AllowedStatuses =
    {
        "DRAFT", "PENDING", "PICKUP_SCHEDULED", "PICKED_UP", "IN_TRANSIT",
        "OUT_FOR_DELIVERY", "DELIVERED", "DELIVERY_FAILED",
        "RETURNED", "CANCELLED"
    };

    public UpdateShipmentStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(s => AllowedStatuses.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}

public class UpdateShipmentPickupValidator : AbstractValidator<UpdateShipmentPickupDto>
{
    public UpdateShipmentPickupValidator()
    {
        RuleFor(x => x.PickedUpAt)
            .NotNull().WithMessage("PickedUpAt is required");
    }
}
