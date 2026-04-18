using ProductService.Infrastructure.Data.Context;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Application.Consumers;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ProductService.Infrastructure.Repositories;
using ProductService.APIService.Middlewares;
using ProductService.APIService.Filters;
using ProductService.APIService.Converters;
using ProductService.APIService.Extensions;
using ProductService.APIService.HostedServices;
using DotNetEnv;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Product")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// .env sớm để PRODUCT_ALLOWED_ORIGINS (nếu có) áp dụng cho CORS
{
    var rootPathEarly = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
    var envPathEarly = Path.Combine(rootPathEarly ?? "", ".env");
    if (File.Exists(envPathEarly))
        Env.Load(envPathEarly);
}

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Product")
    .Filter.ByExcluding(logEvent =>
        logEvent.Exception is { } ex && (
            ex.ToString().Contains("57P01", StringComparison.Ordinal) ||
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase)))
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

// CORS + credentials: dùng SetIsOriginAllowed (không cần liệt kê URL trong appsettings).
var productCorsExtraOrigins = BuildProductCorsExtraOriginSet(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductCors", policy =>
    {
        policy.SetIsOriginAllowed(origin => IsProductCorsOriginAllowed(origin, productCorsExtraOrigins))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Service API", Version = "v1" });
});

builder.Services.AddDbContext<ProductDbContext>();

builder.Services.AddHostedService<ShopNamesRequestPublisherHostedService>();

var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest",
    VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost") ?? builder.Configuration["RabbitMQ:VirtualHost"] ?? "/"
};

// Try to connect to RabbitMQ - wait a bit if it's not ready yet
bool rabbitMQAvailable = false;
for (int i = 0; i < 5; i++)
{
    try
    {
        var publisher = new RabbitMQPublisher(rabbitMQSettings);
        builder.Services.AddSingleton(publisher);
        Console.WriteLine("RabbitMQ Publisher connected successfully");

        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(consumer);
        Console.WriteLine("RabbitMQ Consumer connected successfully");

        // Register event consumers (require RabbitMQ)
        builder.Services.AddSingleton<ShopEventConsumer>();
        builder.Services.AddSingleton<OrderReviewEligibilityConsumer>();
        builder.Services.AddSingleton<OrderDeliveredStockConsumer>();
        builder.Services.AddSingleton<OrderProductMirrorConsumer>();

        rabbitMQAvailable = true;
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"RabbitMQ connection attempt {i + 1} failed: {ex.Message}");
        if (i < 4)
        {
            Console.WriteLine("Retrying in 5 seconds...");
            await Task.Delay(5000);
        }
    }
}

if (!rabbitMQAvailable)
{
    Console.WriteLine("WARNING: RabbitMQ is not available. Event publishing will be disabled.");
    // Register a null publisher to satisfy dependencies
    builder.Services.AddSingleton<RabbitMQPublisher>(sp => null!);
}

builder.Services.AddProductServices();
builder.Services.AddValidators();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

try
{
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetLevel = (httpCtx, _, ex) =>
            ex != null || httpCtx.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error :
            httpCtx.Response.StatusCode >= 400 ? Serilog.Events.LogEventLevel.Warning :
                                                               Serilog.Events.LogEventLevel.Information;
    });

    // ── Clean DB-disconnect handler: log 1 WRN line instead of full stack trace ───
    app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
    {
        var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        var root = ((ex?.InnerException ?? ex)?.Message ?? "Unknown error")
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "Unknown error";

        if (ex is not null && (
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase) ||
            (ex.InnerException?.ToString() ?? ex.ToString()).Contains("57P01", StringComparison.Ordinal)))
        {
            logger.LogWarning("[DB] Connection lost — {Error}. Service will recover on next request.", root);
            ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
        else
        {
            logger.LogError("[{ExType}] {Message}", ex?.GetType().Name ?? "Exception", root);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                error = ctx.Response.StatusCode == 503
                    ? "Service temporarily unavailable. Please retry."
                    : "An unexpected error occurred."
            }));
    }));

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
        c.RoutePrefix = "";
    });

    app.UseValidationExceptionMiddleware();

    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if ((path.StartsWith("/product/", StringComparison.OrdinalIgnoreCase)
             || string.Equals(path, "/product", StringComparison.OrdinalIgnoreCase))
            && !path.StartsWith("/api/product", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.Path = new PathString("/api" + path);
        }

        await next();
    });

    app.UseCors("ProductCors");

    app.UseAuthorization();

    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "product-service" }));
    app.MapGet("/api/product/health", () => Results.Ok(new { status = "healthy", service = "product-service" }));

    // Start RabbitMQ event consumers
    try
    {
        ServiceCollectionExtensions.StartEventConsumers(app.Services);
        Console.WriteLine("[ProductService] Event consumers started successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ProductService] Failed to start event consumers: {ex.Message}");
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to start application: {ex.Message}");
}

static HashSet<string> BuildProductCorsExtraOriginSet(IConfiguration configuration)
{
    var csv = Environment.GetEnvironmentVariable("PRODUCT_ALLOWED_ORIGINS")
        ?? configuration["Cors:AllowedOrigins"];
    var list = ParseCommaSeparated(csv);
    return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
}

static bool IsProductCorsOriginAllowed(string? origin, HashSet<string> extraOrigins)
{
    if (string.IsNullOrEmpty(origin))
        return false;

    if (extraOrigins.Count > 0 && extraOrigins.Contains(origin))
        return true;

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return false;

    if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
        && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
        return true;

    if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
        return true;

    return false;
}

static List<string> ParseCommaSeparated(string? csv)
{
    if (string.IsNullOrWhiteSpace(csv))
        return [];

    return csv
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(s => Uri.TryCreate(s, UriKind.Absolute, out _))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}
