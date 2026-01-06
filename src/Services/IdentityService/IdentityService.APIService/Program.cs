using IdentityService.Infrastructure.Data.Context;
using IdentityService.Application.Consumers;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;
using IdentityService.Infrastructure.Repositories;
using IdentityService.APIService.Extensions;
using IdentityService.APIService.Middlewares;
using IdentityService.APIService.Filters;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identity Service API", Version = "v1" });
});


builder.Services.AddDbContext<AccountDbContext>();


var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};
try
{
    var consumer = new RabbitMQConsumer(rabbitMQSettings);
    builder.Services.AddSingleton(consumer);
    builder.Services.AddHostedService<ShopCreatedConsumer>();
    Console.WriteLine("RabbitMQ Consumer connected successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"RabbitMQ not available: {ex.Message}. Running without messaging.");
}

var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

builder.Services.AddAccountServices();
builder.Services.AddValidators();

var app = builder.Build();

//configure the HTTP request pipeline.
var port = Environment.GetEnvironmentVariable("IDENTITY_SERVICE_PORT");
app.Urls.Add($"http://localhost:{port}");



app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
    c.RoutePrefix = "";
});

app.UseValidationExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "identity-service" }));

app.Run();
