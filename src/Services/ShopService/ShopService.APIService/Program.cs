using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ShopService.APIService.Extensions;
using ShopService.APIService.Middlewares;
using ShopService.APIService.Filters;
using ShopService.Infrastructure.Data.Context;
using ShopService.Infrastructure.Data.Seeders;


var builder = WebApplication.CreateBuilder(args);

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
    Console.WriteLine("RabbitMQ Publisher connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ not available: {ex.Message}. Running without messaging.");
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
        Console.WriteLine("Database migration applied successfully");
        
        // Seed initial data
        await ShopDbSeeder.SeedAsync(dbContext);
        Console.WriteLine("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration/seeding failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
var port = Environment.GetEnvironmentVariable("SHOP_SERVICE_PORT");
app.Urls.Add($"http://localhost:{port}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Service API v1");
    c.RoutePrefix = "";
});

app.UseValidationExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shop-service" }));

app.Run();
