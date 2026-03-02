var builder = WebApplication.CreateBuilder(args);

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
app.UseCors("AllowAll");

// Map YARP reverse proxy endpoints
app.MapReverseProxy();

// Health check endpoint cho API Gateway
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "api-gateway" }));

app.Run();
