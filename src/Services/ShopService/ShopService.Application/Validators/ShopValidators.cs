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

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        RuleFor(x => x.BankAccountNumber)
            .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters")
            .Matches(@"^\d+$").WithMessage("Bank account number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

        RuleFor(x => x.BankAccountName)
            .MaximumLength(100).WithMessage("Bank account name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankAccountName));
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

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        RuleFor(x => x.BankAccountNumber)
            .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters")
            .Matches(@"^\d+$").WithMessage("Bank account number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

        RuleFor(x => x.BankAccountName)
            .MaximumLength(100).WithMessage("Bank account name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankAccountName));
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

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        RuleFor(x => x.BankAccountNumber)
            .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters")
            .Matches(@"^\d+$").WithMessage("Bank account number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

        RuleFor(x => x.BankAccountName)
            .MaximumLength(100).WithMessage("Bank account name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankAccountName));
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

        RuleFor(x => x.BankName)
            .MaximumLength(100).WithMessage("Bank name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankName));

        RuleFor(x => x.BankAccountNumber)
            .MaximumLength(50).WithMessage("Bank account number cannot exceed 50 characters")
            .Matches(@"^\d+$").WithMessage("Bank account number must contain only digits")
            .When(x => !string.IsNullOrEmpty(x.BankAccountNumber));

        RuleFor(x => x.BankAccountName)
            .MaximumLength(100).WithMessage("Bank account name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.BankAccountName));
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
