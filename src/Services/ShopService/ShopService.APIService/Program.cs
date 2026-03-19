using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Messaging;
using ShopService.APIService.Extensions;
using ShopService.APIService.Middlewares;
using ShopService.APIService.Filters;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Data.Seeders;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Shop")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext().Enrich.WithProperty("Service", "Shop").Filter.ByExcluding(logEvent =>
        logEvent.Exception is { } ex && (
            ex.ToString().Contains("57P01", StringComparison.Ordinal) ||
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase)))
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shop Service API", Version = "v1" });
});

builder.Services.AddDbContext<ShopDbContext>();

var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Đăng ký RabbitMQ Publisher (chỉ khi có RabbitMQ)
try
{
    var publisher = new RabbitMQPublisher(rabbitMQSettings);
    builder.Services.AddSingleton(publisher);
}
catch (Exception ex)
{
    // RabbitMQ not available - continue without messaging
}

builder.Services.AddShopServices(builder.Configuration);
builder.Services.AddValidators();

var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    try
    {
        dbContext.Database.Migrate();

        // Seed initial data
        await ShopDbSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        // Log error but continue startup
    }
}

// Configure the HTTP request pipeline.
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Service API v1");
    c.RoutePrefix = "";
});

app.UseValidationExceptionMiddleware();

// Enable CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shop-service" }));

app.Run();
