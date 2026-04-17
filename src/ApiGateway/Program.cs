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

ApplyReverseProxyEnvOverrides(builder.Configuration);

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
        var allowedOrigins = ParseAllowedOrigins(builder.Configuration["GATEWAY_ALLOWED_ORIGINS"]);
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
            return;
        }

        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

static void ApplyReverseProxyEnvOverrides(IConfiguration configuration)
{
    var envToConfigMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["GATEWAY_IDENTITY_URL"] =
            "ReverseProxy:Clusters:identity-cluster:Destinations:identity-destination:Address",
        ["GATEWAY_ROLES_URL"] =
            "ReverseProxy:Clusters:identity-cluster:Destinations:identity-destination:Address",
        ["GATEWAY_ACCOUNTS_URL"] =
            "ReverseProxy:Clusters:identity-cluster:Destinations:identity-destination:Address",
        ["GATEWAY_AUTH_URL"] =
            "ReverseProxy:Clusters:identity-cluster:Destinations:identity-destination:Address",
        ["GATEWAY_SHOP_URL"] =
            "ReverseProxy:Clusters:shop-cluster:Destinations:shop-destination:Address",
        ["GATEWAY_PRODUCT_URL"] =
            "ReverseProxy:Clusters:product-cluster:Destinations:product-destination:Address",
        ["GATEWAY_INVENTORY_URL"] =
            "ReverseProxy:Clusters:inventory-cluster:Destinations:inventory-destination:Address",
        ["GATEWAY_ORDER_URL"] =
            "ReverseProxy:Clusters:order-cluster:Destinations:order-destination:Address",
        ["GATEWAY_PAYMENT_URL"] =
            "ReverseProxy:Clusters:payment-cluster:Destinations:payment-destination:Address",
        ["GATEWAY_SHIPMENT_URL"] =
            "ReverseProxy:Clusters:shipment-cluster:Destinations:shipment-destination:Address"
    };

    foreach (var mapping in envToConfigMap)
    {
        var envValue = Environment.GetEnvironmentVariable(mapping.Key);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            configuration[mapping.Value] = envValue.Trim();
        }
    }
}

static string[] ParseAllowedOrigins(string? originsCsv)
{
    if (string.IsNullOrWhiteSpace(originsCsv))
        return Array.Empty<string>();

    return originsCsv
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(origin => Uri.TryCreate(origin, UriKind.Absolute, out _))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
