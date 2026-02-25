using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ShipmentService.Infrastructure.Data.Context;

var builder = WebApplication.CreateBuilder(args);

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
    c.SwaggerDoc("v1", new() { Title = "Shipment Service API", Version = "v1" });
});

builder.Services.AddDbContext<ShipmentDbContext>();

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

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentDbContext>();
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log error but continue startup
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS
app.UseCors("AllowAll");

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));

app.Run();
