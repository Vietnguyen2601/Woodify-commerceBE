using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Application.Consumers;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.PayOs;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.UnitOfWork;
using Shared.Messaging;
using DotNetEnv;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Payment")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext().Enrich.WithProperty("Service", "Payment").Filter.ByExcluding(logEvent =>
        logEvent.Exception is { } ex && (
            ex.ToString().Contains("57P01", StringComparison.Ordinal) ||
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase)))
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

// ==========================================
// 0. Load .env file
// ==========================================
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
Env.Load(envPath);


// ==========================================
// 1. Controllers & Swagger
// ==========================================
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
    c.SwaggerDoc("v1", new() { Title = "Payment Service API", Version = "v1" });

    // Thêm XML comments nếu có
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ==========================================
// 2. Database - EF Core PostgreSQL
// ==========================================
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PaymentService")
    ?? builder.Configuration.GetConnectionString("PaymentService");

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// 3. PayOS Configuration - IOptions Pattern
// ==========================================
builder.Services.Configure<PayOsOptions>(options =>
{
    // Ưu tiên đọc từ Environment Variables (cho Docker/Production)
    options.ClientId = builder.Configuration["PAYOS_CLIENT_ID"]
        ?? builder.Configuration["PayOs:ClientId"]
        ?? string.Empty;

    options.ApiKey = builder.Configuration["PAYOS_API_KEY"]
        ?? builder.Configuration["PayOs:ApiKey"]
        ?? string.Empty;

    options.ChecksumKey = builder.Configuration["PAYOS_CHECKSUM_KEY"]
        ?? builder.Configuration["PayOs:ChecksumKey"]
        ?? string.Empty;

    options.BaseUrl = builder.Configuration["PAYOS_BASE_URL"]
        ?? builder.Configuration["PayOs:BaseUrl"]
        ?? "https://api-merchant.payos.vn";
});

// ==========================================
// 4. HttpClient for PayOS - HttpClientFactory
// ==========================================
builder.Services.AddHttpClient<IPayOsService, PayOsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ==========================================
// 5. Repositories
// ==========================================
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// ==========================================
// 5.1 Unit of Work
// ==========================================
builder.Services.AddScoped<PaymentService.Application.Interfaces.IUnitOfWork, PaymentService.Infrastructure.UnitOfWork.UnitOfWork>();

// ==========================================
// 6. Application Services
// ==========================================
builder.Services.AddScoped<IPaymentAppService, PaymentAppService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<WalletTopUpService>();
builder.Services.AddScoped<IPayOsWebhookHandler, PayOsWebhookHandler>();

// ==========================================
// 6.5 Payment Callback Configuration
// ==========================================
builder.Services.Configure<PaymentCallbackOptions>(options =>
{
    options.ReturnUrl = Environment.GetEnvironmentVariable("PAYMENT_CALLBACK_RETURN_URL")!;
    options.CancelUrl = Environment.GetEnvironmentVariable("PAYMENT_CALLBACK_CANCEL_URL")!;
});

// ==========================================
// 7. RabbitMQ (Optional - existing configuration)
// ==========================================
var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest",
    VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost") ?? builder.Configuration["RabbitMQ:VirtualHost"] ?? "/"
};

// Retry logic for RabbitMQ connection
RabbitMQConsumer? rabbitMQConsumer = null;
for (int attempt = 1; attempt <= 5; attempt++)
{
    try
    {
        rabbitMQConsumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(rabbitMQConsumer);
        var rabbitMQPublisher = new RabbitMQPublisher(rabbitMQSettings);
        builder.Services.AddSingleton(rabbitMQPublisher);
        builder.Services.AddHostedService<AccountCreatedConsumer>();
        break;
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine($"Failed to initialize RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}

builder.Services.AddSingleton<IPaymentEventsPublisher>(sp =>
    new PaymentEventsPublisher(
        sp.GetService<RabbitMQPublisher>(),
        sp.GetRequiredService<ILogger<PaymentEventsPublisher>>()));

// ==========================================
// 8. Build App
// ==========================================
var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log error but continue startup
    }
}

// ==========================================
// 9. Configure Port
// ==========================================
var port = Environment.GetEnvironmentVariable("PAYMENT_SERVICE_PORT") ?? "5015";
app.Urls.Add($"http://*:{port}");

// ==========================================
// 10. Middleware Pipeline
// ==========================================

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

// Enable request body buffering for webhook signature verification
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Service API v1");
    c.RoutePrefix = "";
});

// Enable CORS
app.UseCors("AllowAll");

// Map Controllers
app.MapControllers();

// Health Check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "payment-service" }));

// ==========================================
// 10. Log Configuration
// ==========================================

app.Run();
