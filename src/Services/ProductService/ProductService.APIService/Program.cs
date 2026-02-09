using ProductService.Infrastructure.Data.Context;
using ProductService.Infrastructure.Data.Seeders;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ProductService.Infrastructure.Repositories;
using ProductService.APIService.Middlewares;
using ProductService.APIService.Filters;
using ProductService.APIService.Converters;
using ProductService.APIService.Extensions;
using DotNetEnv;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    options.JsonSerializerOptions.Converters.Add(new NullableGuidConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product Service API", Version = "v1" });
});

builder.Services.AddDbContext<ProductDbContext>();


var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Try to connect to RabbitMQ - wait a bit if it's not ready yet
bool rabbitMQAvailable = false;
for (int i = 0; i < 5; i++)
{
    try
    {
        var publisher = new RabbitMQPublisher(rabbitMQSettings);
        builder.Services.AddSingleton(publisher);
        Console.WriteLine("RabbitMQ Publisher connected successfully");
        
        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(consumer);
        Console.WriteLine("RabbitMQ Consumer connected successfully");
        
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
    Console.WriteLine("WARNING: RabbitMQ is not available. Event publishing will be disabled.");
    // Register a null publisher to satisfy dependencies
    builder.Services.AddSingleton<RabbitMQPublisher>(sp => null!);
}

var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

builder.Services.AddProductServices();
builder.Services.AddValidators();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database migration applied successfully");
        
        // Seed initial data
        await ProductDbSeeder.SeedAsync(dbContext);
        Console.WriteLine("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration/seeding failed: {ex.Message}");
    }
}

try
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
        c.RoutePrefix = "";
    });

    app.UseValidationExceptionMiddleware();

    app.UseAuthorization();

    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "product-service" }));
    app.MapGet("/api/product/health", () => Results.Ok(new { status = "healthy", service = "product-service" }));

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to start application: {ex.Message}");
}
