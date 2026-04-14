using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Shared.Messaging;
using OrderService.Infrastructure.Data.Context;
using OrderService.Infrastructure.Data.Seeders;
using OrderService.APIService.Extensions;
using OrderService.APIService.Services;
using OrderService.Application.Consumers;
using OrderService.Application.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Order")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext().Enrich.WithProperty("Service", "Order").Filter.ByExcluding(logEvent =>
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
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
});

// Add SignalR for real-time dashboard metrics with extended timeout
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // Client timeout 60s (default 30s)
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32MB max message size
});

builder.Services.AddDbContext<OrderDbContext>();

// Register Order Services
builder.Services.AddOrderServices(builder.Configuration);

// Register MetricsPublisher background service (for real-time dashboard)
builder.Services.AddHostedService<MetricsPublisherService>();

var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest",
    VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost") ?? builder.Configuration["RabbitMQ:VirtualHost"] ?? "/"
};

// Try to connect to RabbitMQ with retry logic
bool rabbitMQAvailable = false;
for (int i = 0; i < 5; i++)
{
    try
    {
        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(consumer);

        var publisher = new RabbitMQPublisher(rabbitMQSettings);
        builder.Services.AddSingleton(publisher);

        // Register OrderEventPublisher and Event Consumers
        builder.Services.AddSingleton<OrderEventPublisher>();
        builder.Services.AddSingleton<ProductEventConsumer>();
        builder.Services.AddSingleton<ShippingFeeEventConsumer>();
        builder.Services.AddSingleton<ShopEventConsumer>();
        builder.Services.AddSingleton<PaymentOrdersPaidConsumer>();

        Console.WriteLine("RabbitMQ Publisher and Consumer connected successfully");
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
    Console.WriteLine("WARNING: RabbitMQ is not available. Event consumption will be disabled.");
}

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration applied successfully");

        // Seed initial data
        await OrderDbSeeder.SeedAsync(dbContext);
        Console.WriteLine("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration/seeding failed: {ex.Message}");
    }
}

// Start Product Event Consumer
try
{
    ServiceCollectionExtensions.StartEventConsumers(app.Services);
    Console.WriteLine("Product Event Consumer started successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to start Product Event Consumer: {ex.Message}");
}

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

// Enable static file serving for wwwroot (HTML, CSS, JS, etc.)
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    c.RoutePrefix = "";
});

// Enable CORS
app.UseCors("AllowAll");

app.MapControllers();

// Map SignalR Hubs for real-time dashboard
app.MapHub<OrderService.APIService.Hubs.DashboardHub>("/admin-dashboard-hub");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "order-service" }));

app.Run();
