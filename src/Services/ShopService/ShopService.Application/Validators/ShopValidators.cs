using FluentValidation;
using ShopService.Application.DTOs;

namespace ShopService.Application.Validators;

public class CreateShopValidator : AbstractValidator<CreateShopDto>
{
    public CreateShopValidator()
    {
        RuleFor(x => x.ShopName)
            .NotEmpty().WithMessage("Shop name is required")
            .MaximumLength(100).WithMessage("Shop name cannot exceed 100 characters");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10,15}$").WithMessage("Phone number must be 10-15 digits").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

public class UpdateShopValidator : AbstractValidator<UpdateShopDto>
{
    public UpdateShopValidator()
    {
        RuleFor(x => x.ShopName)
            .NotEmpty().WithMessage("Shop name is required")
            .MaximumLength(100).WithMessage("Shop name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10,15}$").WithMessage("Phone number must be 10-15 digits").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
