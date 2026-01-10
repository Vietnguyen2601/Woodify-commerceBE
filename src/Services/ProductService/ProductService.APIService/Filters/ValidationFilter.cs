using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductService.APIService.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.Any())
        {
            await next();
            return;
        }

        var argument = context.ActionArguments.First().Value;
        
        if (argument == null)
        {
            await next();
            return;
        }

        var argumentType = argument.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        if (validator == null)
        {
            await next();
            return;
        }

        var validationContext = new ValidationContext<object>(argument);
        var validationResult = await validator.ValidateAsync(validationContext);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = "Validation failed",
                errors = errors
            });
            return;
        }

        await next();
    }
}
