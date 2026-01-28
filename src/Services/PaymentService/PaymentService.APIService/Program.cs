using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.PayOs;
using PaymentService.Infrastructure.Repositories;
using Shared.Messaging;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 0. Load .env file
// ==========================================
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
Env.Load(envPath);


// ==========================================
// 1. Controllers & Swagger
// ==========================================
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

Console.WriteLine($"Connection String loaded: {!string.IsNullOrEmpty(connectionString)}");

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
// 6. Application Services
// ==========================================
builder.Services.AddScoped<IPaymentAppService, PaymentAppService>();
builder.Services.AddScoped<IWalletService, WalletService>();

// ==========================================
// 7. RabbitMQ (Optional - existing configuration)
// ==========================================
var rabbitMQSettings = new RabbitMQSettings
{
    Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

try
{
    var consumer = new RabbitMQConsumer(rabbitMQSettings);
    builder.Services.AddSingleton(consumer);
    Console.WriteLine("RabbitMQ Consumer connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ not available: {ex.Message}. Running without messaging.");
}

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
        Console.WriteLine("Database migration applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
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

// Map Controllers
app.MapControllers();

// Health Check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "payment-service" }));

// ==========================================
// 10. Log Configuration
// ==========================================
var payOsClientId = builder.Configuration["PAYOS_CLIENT_ID"] ?? builder.Configuration["PayOs:ClientId"];
Console.WriteLine($"PayOS ClientId configured: {!string.IsNullOrEmpty(payOsClientId)}");
Console.WriteLine($"Connection String configured: {!string.IsNullOrEmpty(connectionString)}");

app.Run();
