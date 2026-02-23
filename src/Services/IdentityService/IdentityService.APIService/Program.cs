using IdentityService.Infrastructure.Data.Context;
using IdentityService.Infrastructure.Data.Seeders;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always; // HTTPS only in production
});

// Add CORS configuration with credential support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
        // Note: Cannot use AllowCredentials() with AllowAnyOrigin()
        // Will configure properly later with specific origins
    });
});

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
    c.SwaggerDoc("v1", new() { Title = "Identity Service API", Version = "v1" });

    // JWT Bearer token support in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<AccountDbContext>();


var rabbitMQSettings = new RabbitMQSettings
{
    Host = Environment.GetEnvironmentVariable("RabbitMQ_Host") ?? builder.Configuration["RabbitMQ:Host"] ?? "localhost",
    Port = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port") ?? builder.Configuration["RabbitMQ:Port"] ?? "5672"),
    Username = Environment.GetEnvironmentVariable("RabbitMQ_Username") ?? builder.Configuration["RabbitMQ:Username"] ?? "guest",
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest"
};

// Retry logic for RabbitMQ connection
RabbitMQConsumer? rabbitMQConsumer = null;
RabbitMQPublisher? rabbitMQPublisher = null;

for (int attempt = 1; attempt <= 5; attempt++)
{
    try
    {
        rabbitMQConsumer = new RabbitMQConsumer(rabbitMQSettings);
        rabbitMQPublisher = new RabbitMQPublisher(rabbitMQSettings);
        
        builder.Services.AddSingleton(rabbitMQConsumer);
        builder.Services.AddSingleton(rabbitMQPublisher);
        builder.Services.AddHostedService<ShopCreatedConsumer>();
        break;
    }
    catch (Exception ex)
    {
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
}

var rootPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
var envPath = Path.Combine(rootPath ?? "", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

builder.Services.AddAccountServices();
builder.Services.AddValidators();

// JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"] ?? "default-secret-key-change-in-production-32chars";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "WoodifyBE";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "WoodifyApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    try
    {
        dbContext.Database.Migrate();
        
        // Seed initial data
        await AccountDbSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        // Log error but continue startup
    }
}

try
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
        c.RoutePrefix = "";
    });

    app.UseValidationExceptionMiddleware();

    // Use JWT Cookie Middleware - Extract token from cookie and add to Authorization header
    app.UseJwtCookieMiddleware();

    // Use Cookie Policy
    app.UseCookiePolicy();

    // Enable CORS
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "identity-service" }));
    app.MapGet("/api/identity/health", () => Results.Ok(new { status = "healthy", service = "identity-service" }));

    app.Run();
}
catch (Exception ex)
{
}
