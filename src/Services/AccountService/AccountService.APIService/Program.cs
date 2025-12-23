using AccountService.Infrastructure.Data.Context;
using AccountService.Infrastructure.Repositories;
using AccountService.Infrastructure.Repositories.IRepositories;
using AccountService.Infrastructure.UnitOfWork;
using AccountService.Application.Consumers;
using AccountService.Application.Interfaces;
using AccountService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SERVICES CONFIGURATION
// ========================================

// Add controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Account Service API", Version = "v1" });
});

// ========================================
// DATABASE CONFIGURATION
// ========================================
// Connection string từ appsettings.json hoặc environment variable
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================================
// RABBITMQ CONFIGURATION
// ========================================
var rabbitMQSettings = new RabbitMQSettings
{
    Host = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Đăng ký RabbitMQ Consumer (chỉ khi có RabbitMQ)
try
{
    var consumer = new RabbitMQConsumer(rabbitMQSettings);
    builder.Services.AddSingleton(consumer);
    builder.Services.AddHostedService<ShopCreatedConsumer>();
    Console.WriteLine("✅ RabbitMQ Consumer connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ RabbitMQ not available: {ex.Message}. Running without messaging.");
}

// ========================================
// DEPENDENCY INJECTION
// ========================================
// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Services (using fully qualified name to avoid conflict with namespace)
builder.Services.AddScoped<IAccountService>(sp => 
    new global::AccountService.Application.Services.AccountService(
        sp.GetRequiredService<IAccountRepository>(),
        sp.GetRequiredService<IRoleRepository>()
    ));
builder.Services.AddScoped<IRoleService, RoleService>();

var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

// Swagger UI - enable cho development và testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "account-service" }));

// ========================================
// AUTO MIGRATION (Development only)
// ========================================
// Tự động apply migrations khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    db.Database.Migrate();
}

app.Run();
