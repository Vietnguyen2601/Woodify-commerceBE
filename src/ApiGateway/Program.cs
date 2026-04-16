using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, _, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code));

// ================================================================
// YARP REVERSE PROXY CONFIGURATION
// ================================================================
// Load cấu hình từ appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS cho phép frontend gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
               .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    opts.GetLevel = (httpCtx, _, ex) =>
        ex != null || httpCtx.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error   :
        httpCtx.Response.StatusCode >= 400               ? Serilog.Events.LogEventLevel.Warning :
                                                           Serilog.Events.LogEventLevel.Information;
});
app.UseCors("AllowAll");

// Map YARP reverse proxy endpoints
app.MapReverseProxy();

// Health check endpoint cho API Gateway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "api-gateway" }));

app.Run();
