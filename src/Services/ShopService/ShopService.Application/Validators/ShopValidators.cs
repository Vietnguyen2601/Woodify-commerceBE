using FluentValidation;
using ShopService.Application.DTOs;

namespace ShopService.Application.Validators;

public class RegisterShopValidator : AbstractValidator<RegisterShopDto>
{
    public RegisterShopValidator()
    {
        RuleFor(x => x.OwnerAccountId)
            .NotEmpty().WithMessage("Owner account ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required")
            .MaximumLength(200).WithMessage("Shop name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.LogoUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Logo URL must be a valid Cloudinary URL");

        RuleFor(x => x.CoverImageUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Cover image URL must be a valid Cloudinary URL");
    }
}

public class UpdateShopInfoValidator : AbstractValidator<UpdateShopInfoDto>
{
    public UpdateShopInfoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Shop name cannot exceed 200 characters")
            .NotEmpty().WithMessage("Shop name cannot be empty if provided")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.LogoUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Logo URL must be a valid Cloudinary URL");

        RuleFor(x => x.CoverImageUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Cover image URL must be a valid Cloudinary URL");
    }
}

public class CreateShopValidator : AbstractValidator<CreateShopDto>
{
    public CreateShopValidator()
    {
        RuleFor(x => x.OwnerAccountId)
            .NotEmpty().WithMessage("Owner account ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required")
            .MaximumLength(200).WithMessage("Shop name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.LogoUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Logo URL must be a valid Cloudinary URL");

        RuleFor(x => x.CoverImageUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Cover image URL must be a valid Cloudinary URL");
    }
}

public class UpdateShopValidator : AbstractValidator<UpdateShopDto>
{
    public UpdateShopValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required")
            .MaximumLength(200).WithMessage("Shop name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.LogoUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Logo URL must be a valid Cloudinary URL");

        RuleFor(x => x.CoverImageUrl)
            .Must(url => url == null || url.StartsWith("https://res.cloudinary.com/"))
            .WithMessage("Cover image URL must be a valid Cloudinary URL");
    }
}

public class UpdateShopStatusValidator : AbstractValidator<UpdateShopStatusDto>
{
    private static readonly string[] AllowedStatuses = { "ACTIVE", "INACTIVE", "SUSPENDED", "BANNED" };

    public UpdateShopStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("new_status is required")
            .Must(s => AllowedStatuses.Contains(s?.ToUpper()))
            .WithMessage($"new_status must be one of: {string.Join(", ", AllowedStatuses)}");


    }
}
