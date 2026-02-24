using FluentValidation;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Validators;

public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 50)
            .WithMessage("Username must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number")
            .Matches(@"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]")
            .WithMessage("Password must contain at least one special character");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+84|0)[0-9]{9}$")
            .WithMessage("Phone number is not valid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Dob)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date of birth cannot be in the future")
            .When(x => x.Dob.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => g == null || new[] { "Male", "Female", "Other" }.Contains(g))
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("RoleId is required")
            .When(x => x.RoleId.HasValue);
    }
}

public class UpdateAccountValidator : AbstractValidator<UpdateAccountDto>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+84|0)[0-9]{9}$")
            .WithMessage("Phone number is not valid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Dob)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date of birth cannot be in the future")
            .When(x => x.Dob.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => g == null || new[] { "Male", "Female", "Other" }.Contains(g))
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }
}

public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .WithMessage("Role name is required")
            .Length(3, 100)
            .WithMessage("Role name must be between 3 and 100 characters")
            .Matches(@"^[a-zA-Z0-9_\s-]+$")
            .WithMessage("Role name can only contain letters, numbers, underscores, spaces, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.RoleName)
            .Length(3, 100)
            .WithMessage("Role name must be between 3 and 100 characters")
            .Matches(@"^[a-zA-Z0-9_\s-]+$")
            .WithMessage("Role name can only contain letters, numbers, underscores, spaces, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.RoleName));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
