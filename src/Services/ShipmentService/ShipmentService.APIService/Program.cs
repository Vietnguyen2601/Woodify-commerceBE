using System.IO;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Messaging;
using ShipmentService.APIService.Extensions;
using ShipmentService.APIService.Filters;
using ShipmentService.APIService.Middlewares;
using ShipmentService.Application.Consumers;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Data.Seeders;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Shipment")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext().Enrich.WithProperty("Service", "Shipment").Filter.ByExcluding(logEvent =>
        logEvent.Exception is { } ex && (
            ex.ToString().Contains("57P01", StringComparison.Ordinal) ||
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase)))
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

// ── Load .env file ────────────────────────────────────────────────────────────
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Join(rootPath ?? "", ".env");
if (File.Exists(envPath)) Env.Load(envPath);

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5010")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shipment Service API", Version = "v1" });
});

// ── DbContext ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ShipmentDbContext>();

// ── Application + Repository Services ────────────────────────────────────────
builder.Services.AddShipmentServices();
builder.Services.AddValidators();

// ── GHN Shipping API Client ───────────────────────────────────────────────────
builder.Services.AddShippingFeeCalculator();

// ── In-Memory Cache ───────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── RabbitMQ (retry giống IdentityService / PaymentService) ─────────────────
var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

for (int attempt = 1; attempt <= 5; attempt++)
{
    try
    {
        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        var publisher = new RabbitMQPublisher(rabbitMQSettings);

        builder.Services.AddSingleton(consumer);
        builder.Services.AddSingleton(publisher);
        builder.Services.AddSingleton<ShipmentService.Application.Services.ShipmentEventPublisher>();
        builder.Services.AddHostedService<OrderEventConsumer>();
        builder.Services.AddHostedService<ShopEventConsumer>();
        break;
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine($"Failed to initialize RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
            Thread.Sleep(5000);
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
            Thread.Sleep(5000);
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
            Thread.Sleep(5000);
    }
}

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentDbContext>();
    try
    {
        dbContext.Database.Migrate();

        // Seed initial data
        await ShipmentDbSeeder.SeedAsync(dbContext);
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
    {
        Console.Error.WriteLine($"Migration failed due to database update error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Migration failed: {ex.Message}");
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipment Service API v1");
    c.RoutePrefix = "";
});

app.UseValidationExceptionMiddleware();
app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));
app.MapGet("/api/shipment/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));

app.Run();
