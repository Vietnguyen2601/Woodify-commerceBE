using FluentValidation;
using System.Net;
using System.Text.Json;

namespace ShipmentService.APIService.Middlewares;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation exception: {Message}", ex.Message);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (OperationCanceledException)
        {
            // Let request cancellations propagate without being treated as internal server errors.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(e => e.ErrorMessage).ToArray());

        return context.Response.WriteAsJsonAsync(new
        {
            statusCode = context.Response.StatusCode,
            message = "Validation failed",
            errors = errors
        });
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsJsonAsync(new
        {
            statusCode = context.Response.StatusCode,
            message = "An internal server error occurred",
            error = exception.Message
        });
    }
}

public static class ValidationExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseValidationExceptionMiddleware(this IApplicationBuilder builder)
        => builder.UseMiddleware<ValidationExceptionMiddleware>();
}
