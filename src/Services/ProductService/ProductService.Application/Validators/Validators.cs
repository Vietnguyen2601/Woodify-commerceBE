using FluentValidation;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Validators;

public class CreateProductMasterValidator : AbstractValidator<CreateProductMasterDto>
{
    public CreateProductMasterValidator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty()
            .WithMessage("Shop ID is required");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required");

        RuleFor(x => x.GlobalSku)
            .MaximumLength(255)
            .WithMessage("Global SKU cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.GlobalSku));

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid product status");
    }
}

public class UpdateProductMasterValidator : AbstractValidator<UpdateProductMasterDto>
{
    public UpdateProductMasterValidator()
    {
        RuleFor(x => x.GlobalSku)
            .MaximumLength(255)
            .WithMessage("Global SKU cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.GlobalSku));

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid product status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.AvgRating)
            .InclusiveBetween(0, 5)
            .WithMessage("Average rating must be between 0 and 5")
            .When(x => x.AvgRating.HasValue);

        RuleFor(x => x.ReviewCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Review count must be greater than or equal to 0")
            .When(x => x.ReviewCount.HasValue);
    }
}

public class CreateProductVersionValidator : AbstractValidator<CreateProductVersionDto>
{
    public CreateProductVersionValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PriceCents)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0")
            .When(x => x.PriceCents.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .MaximumLength(10)
            .WithMessage("Currency cannot exceed 10 characters");

        RuleFor(x => x.Sku)
            .MaximumLength(255)
            .WithMessage("SKU cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Sku));
    }
}

public class UpdateProductVersionValidator : AbstractValidator<UpdateProductVersionDto>
{
    public UpdateProductVersionValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.PriceCents)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0")
            .When(x => x.PriceCents.HasValue);

        RuleFor(x => x.Currency)
            .MaximumLength(10)
            .WithMessage("Currency cannot exceed 10 characters")
            .When(x => !string.IsNullOrEmpty(x.Currency));

        RuleFor(x => x.Sku)
            .MaximumLength(255)
            .WithMessage("SKU cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Sku));
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(255)
            .WithMessage("Category name cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255)
            .WithMessage("Category name cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public class CreateProductReviewValidator : AbstractValidator<CreateProductReviewDto>
{
    public CreateProductReviewValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars");

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}

public class UpdateProductReviewValidator : AbstractValidator<UpdateProductReviewDto>
{
    public UpdateProductReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .WithMessage("Title cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.HelpfulCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Helpful count must be greater than or equal to 0")
            .When(x => x.HelpfulCount.HasValue);
    }
}
