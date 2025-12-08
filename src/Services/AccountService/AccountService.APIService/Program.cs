using AccountService.Repositories.DBContext;
using AccountService.Repositories.IRepositories;
using AccountService.Repositories.Repositories;
using AccountService.Repositories.UnitOfWork;
using AccountService.Services.Interfaces;
using AccountService.Services.InternalServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SERVICES CONFIGURATION
// ========================================

// Add controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Account Service API", Version = "v1" });
});

// ========================================
// DATABASE CONFIGURATION
// ========================================
// Connection string từ appsettings.json hoặc environment variable
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================================
// DEPENDENCY INJECTION
// ========================================
// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Services (using alias to avoid conflict with namespace)
builder.Services.AddScoped<IAccountService, AccountService.Services.InternalServices.AccountService>();
builder.Services.AddScoped<IRoleService, RoleService>();

var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

// Swagger UI - enable cho development và testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "account-service" }));

// ========================================
// AUTO MIGRATION (Development only)
// ========================================
// Tự động apply migrations khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    db.Database.Migrate();
}

app.Run();
