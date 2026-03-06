using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using ShipmentService.APIService.Extensions;
using ShipmentService.APIService.Filters;
using ShipmentService.APIService.Middlewares;
using ShipmentService.Infrastructure.Data.Context;
using ShipmentService.Infrastructure.Data.Seeders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Load .env file ────────────────────────────────────────────────────────────
var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Join(rootPath ?? "", ".env");
if (File.Exists(envPath)) Env.Load(envPath);

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5010")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Shipment Service API", Version = "v1" });
});

// ── DbContext ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ShipmentDbContext>();

// ── Application + Repository Services ────────────────────────────────────────
builder.Services.AddShipmentServices();
builder.Services.AddValidators();

// ── In-Memory Cache ───────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── RabbitMQ (with retry) ─────────────────────────────────────────────────────
var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

for (int attempt = 1; attempt <= 5; attempt++)
{
    try
    {
        var consumer = new RabbitMQConsumer(rabbitMQSettings);
        var publisher = new RabbitMQPublisher(rabbitMQSettings);
        builder.Services.AddSingleton(consumer);
        builder.Services.AddSingleton(publisher);
        break;
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"RabbitMQ attempt {attempt} failed: {ex.Message}");
        if (attempt < 5) Thread.Sleep(5000);
    }
    catch (TimeoutException ex)
    {
        Console.Error.WriteLine($"RabbitMQ attempt {attempt} timed out: {ex.Message}");
        if (attempt < 5) Thread.Sleep(5000);
    }
    catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
    {
        Console.Error.WriteLine($"RabbitMQ attempt {attempt} failed: {ex.Message}");
        if (attempt < 5) Thread.Sleep(5000);
    }
}

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Auto-migrate on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ShipmentDbContext>();
    try
    {
        dbContext.Database.Migrate();

        // Seed initial data
        await ShipmentDbSeeder.SeedAsync(dbContext);
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
    {
        Console.Error.WriteLine($"Migration failed due to database update error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Migration failed: {ex.Message}");
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipment Service API v1");
    c.RoutePrefix = "";
});

app.UseValidationExceptionMiddleware();
app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));
app.MapGet("/api/shipment/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));

app.Run();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "shipment-service" }));

app.Run();
