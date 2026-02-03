using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using OrderService.Infrastructure.Data.Context;
using OrderService.APIService.Extensions;
using OrderService.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Service API", Version = "v1" });
});

builder.Services.AddDbContext<OrderDbContext>();

// Register Order Services
builder.Services.AddOrderServices();

var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Try to connect to RabbitMQ with retry logic
bool rabbitMQAvailable = false;
for (int i = 0; i < 5; i++)
{
    try
    {
        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        builder.Services.AddSingleton(consumer);
        
        // Register ProductEventConsumer
        builder.Services.AddSingleton<ProductEventConsumer>();
        
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
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

app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        c.RoutePrefix = "";
    });

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "order-service" }));

app.Run();
