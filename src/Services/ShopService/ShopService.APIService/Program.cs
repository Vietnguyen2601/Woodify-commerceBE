using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Shared.Messaging;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.UnitOfWork;
using ShopService.Application.Interfaces;
using InternalShopService = ShopService.Application.Services.ShopService;

var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// ================================================================
// DATABASE CONFIGURATION
// ================================================================
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseNpgsql(connectionString));

// ================================================================
// RABBITMQ CONFIGURATION
// ================================================================
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
    Console.WriteLine("RabbitMQ Publisher connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ not available: {ex.Message}. Running without messaging.");
}

// ================================================================
// DEPENDENCY INJECTION
// ================================================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IShopService>(sp =>
{
    var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
    var publisher = sp.GetService<RabbitMQPublisher>(); // Có thể null nếu RabbitMQ không available
    return new InternalShopService(unitOfWork, publisher);
});

// ================================================================
// API CONFIGURATION
// ================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

//configure the HTTP request pipeline.
var port = Environment.GetEnvironmentVariable("SHOP_SERVICE_PORT");
if (string.IsNullOrEmpty(port))
{
    throw new InvalidOperationException("SHOP_SERVICE_PORT not found in .env file");
}
app.Urls.Add($"http://localhost:{port}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Service API v1");
    c.RoutePrefix = "";
});

app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shop-service" }));

app.Run();
