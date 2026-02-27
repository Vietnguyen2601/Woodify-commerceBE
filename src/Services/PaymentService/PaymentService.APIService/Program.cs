using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Application.Consumers;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Data.Seeders;
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
// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5015")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
// 6. Application Services
// ==========================================
builder.Services.AddScoped<IPaymentAppService, PaymentAppService>();
builder.Services.AddScoped<IWalletService, WalletService>();

// ==========================================
// 7. RabbitMQ (Optional - existing configuration)
// ==========================================
var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Retry logic for RabbitMQ connection
RabbitMQConsumer? rabbitMQConsumer = null;
for (int attempt = 1; attempt <= 5; attempt++)
{
    try
    {
        rabbitMQConsumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(rabbitMQConsumer);
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
        
        // Seed initial data
        await PaymentDbSeeder.SeedAsync(dbContext);
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
