using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ShopService.Repositories.DBContext;
using ShopService.Repositories.UnitOfWork;
using ShopService.Services.Interfaces;
using InternalShopService = ShopService.Services.InternalServices.ShopService;

var builder = WebApplication.CreateBuilder(args);

// ================================================================
// DATABASE CONFIGURATION
// ================================================================
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================================================
// RABBITMQ CONFIGURATION
// ================================================================
var rabbitMQSettings = new RabbitMQSettings
{
    Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Đăng ký RabbitMQ Publisher (chỉ khi có RabbitMQ)
try
{
    var publisher = new RabbitMQPublisher(rabbitMQSettings);
    builder.Services.AddSingleton(publisher);
    Console.WriteLine("✅ RabbitMQ Publisher connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ RabbitMQ not available: {ex.Message}. Running without messaging.");
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

// Swagger cho tất cả environments
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shop-service" }));

app.Run();
