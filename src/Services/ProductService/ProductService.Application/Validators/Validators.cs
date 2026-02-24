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

        RuleFor(x => x.SellerSku)
            .NotEmpty()
            .WithMessage("Seller SKU is required")
            .MaximumLength(255)
            .WithMessage("Seller SKU cannot exceed 255 characters");

        RuleFor(x => x.VersionName)
            .MaximumLength(500)
            .WithMessage("Version name cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.VersionName));

        RuleFor(x => x.PriceCents)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.BasePriceCents)
            .GreaterThan(0)
            .WithMessage("Base price must be greater than 0")
            .When(x => x.BasePriceCents.HasValue);

        RuleFor(x => x.WeightGrams)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0");

        RuleFor(x => x.LengthCm)
            .GreaterThan(0)
            .WithMessage("Length must be greater than 0");

        RuleFor(x => x.WidthCm)
            .GreaterThan(0)
            .WithMessage("Width must be greater than 0");

        RuleFor(x => x.HeightCm)
            .GreaterThan(0)
            .WithMessage("Height must be greater than 0");

        RuleFor(x => x.BulkyType)
            .Must(bt => bt == null || new[] { "NORMAL", "BULKY", "SUPER_BULKY" }.Contains(bt))
            .WithMessage("Bulky type must be NORMAL, BULKY, or SUPER_BULKY")
            .When(x => !string.IsNullOrEmpty(x.BulkyType));

        RuleFor(x => x.WarrantyMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Warranty months must be greater than or equal to 0");
    }
}

public class UpdateProductVersionValidator : AbstractValidator<UpdateProductVersionDto>
{
    public UpdateProductVersionValidator()
    {
        RuleFor(x => x.SellerSku)
            .MaximumLength(255)
            .WithMessage("Seller SKU cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.SellerSku));

        RuleFor(x => x.VersionName)
            .MaximumLength(500)
            .WithMessage("Version name cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.VersionName));

        RuleFor(x => x.PriceCents)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .When(x => x.PriceCents.HasValue);

        RuleFor(x => x.BasePriceCents)
            .GreaterThan(0)
            .WithMessage("Base price must be greater than 0")
            .When(x => x.BasePriceCents.HasValue);

        RuleFor(x => x.WeightGrams)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0")
            .When(x => x.WeightGrams.HasValue);

        RuleFor(x => x.LengthCm)
            .GreaterThan(0)
            .WithMessage("Length must be greater than 0")
            .When(x => x.LengthCm.HasValue);

        RuleFor(x => x.WidthCm)
            .GreaterThan(0)
            .WithMessage("Width must be greater than 0")
            .When(x => x.WidthCm.HasValue);

        RuleFor(x => x.HeightCm)
            .GreaterThan(0)
            .WithMessage("Height must be greater than 0")
            .When(x => x.HeightCm.HasValue);

        RuleFor(x => x.BulkyType)
            .Must(bt => bt == null || new[] { "NORMAL", "BULKY", "SUPER_BULKY" }.Contains(bt))
            .WithMessage("Bulky type must be NORMAL, BULKY, or SUPER_BULKY")
            .When(x => !string.IsNullOrEmpty(x.BulkyType));

        RuleFor(x => x.WarrantyMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Warranty months must be greater than or equal to 0")
            .When(x => x.WarrantyMonths.HasValue);
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

        RuleFor(x => x.Content)
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}