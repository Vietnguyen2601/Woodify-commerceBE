using IdentityService.Infrastructure.Data.Context;
using IdentityService.Infrastructure.Data.Seeders;
using IdentityService.Application.Consumers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
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

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "Identity")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Identity")
    .Filter.ByExcluding(logEvent =>
        logEvent.Exception is { } ex && (
            ex.ToString().Contains("57P01", StringComparison.Ordinal) ||
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase)))
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

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
        policy.WithOrigins("http://localhost:5173", "https://localhost:5010")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
    Password = Environment.GetEnvironmentVariable("RabbitMQ_Password") ?? builder.Configuration["RabbitMQ:Password"] ?? "guest",
    VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost") ?? builder.Configuration["RabbitMQ:VirtualHost"] ?? "/"
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
        builder.Services.AddSingleton<AccountNamesRequestConsumer>();
        break;
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine($"Failed to initialize RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
    catch (InvalidOperationException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
        if (attempt < 5)
        {
            System.Threading.Thread.Sleep(5000);
        }
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine($"Unexpected error initializing RabbitMQ on attempt {attempt}: {ex.Message}");
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
        
        await RoleDbSeeder.SeedAsync(dbContext);
    }
    catch (Exception)
    {
        // Log error but continue startup
    }
}

try
{
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetLevel = (httpCtx, _, ex) =>
            ex != null || httpCtx.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error   :
            httpCtx.Response.StatusCode >= 400               ? Serilog.Events.LogEventLevel.Warning :
                                                               Serilog.Events.LogEventLevel.Information;
    });

    // ── Clean DB-disconnect handler: log 1 WRN line instead of full stack trace ───
    app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
    {
        var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        var root = ((ex?.InnerException ?? ex)?.Message ?? "Unknown error")
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "Unknown error";

        if (ex is not null && (
            ex.Message.Contains("transient failure", StringComparison.OrdinalIgnoreCase) ||
            (ex.InnerException?.ToString() ?? ex.ToString()).Contains("57P01", StringComparison.Ordinal)))
        {
            logger.LogWarning("[DB] Connection lost — {Error}. Service will recover on next request.", root);
            ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
        else
        {
            logger.LogError("[{ExType}] {Message}", ex?.GetType().Name ?? "Exception", root);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(new
            {
                error = ctx.Response.StatusCode == 503
                    ? "Service temporarily unavailable. Please retry."
                    : "An unexpected error occurred."
            }));
    }));

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

    // Start RabbitMQ event consumers
    try
    {
        var accountNamesRequestConsumer = app.Services.GetService<AccountNamesRequestConsumer>();
        accountNamesRequestConsumer?.StartListening();
    }
    catch (InvalidOperationException ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[IdentityService] Failed to start AccountNamesRequestConsumer due to invalid operation.");
    }
    catch (System.IO.IOException ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[IdentityService] Failed to start AccountNamesRequestConsumer due to I/O error.");
    }

    app.Run();
}
catch (Exception)
{
    // Log startup error for debugging
}
