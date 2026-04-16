using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure.Data.Context;

public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    // Constructor mặc định cho design-time (migrations)
    public AccountDbContext() : base()
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }

    private static string GetConnectionString(string connectionStringName)
    {
        var rootEnvPath = FindEnvFile();
        if (rootEnvPath != null && File.Exists(rootEnvPath))
        {
            foreach (var line in File.ReadAllLines(rootEnvPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
            }
        }

        // Kiểm tra biến môi trường trước
        string envConnectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return config.GetConnectionString(connectionStringName);
    }

    private static string FindEnvFile()
    {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null)
        {
            var envFile = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envFile))
                return envFile;
            dir = dir.Parent;
        }
        return null;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString("IdentityService"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // Cấu hình bảng Roles
        // ========================================
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName).HasColumnName("role_name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        // ========================================
        // Cấu hình bảng Accounts
        // ========================================
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.AccountId);

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // Unique constraints
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Relationship với Role
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Accounts)
                  .HasForeignKey(e => e.RoleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tự động cập nhật UpdatedAt khi save và đảm bảo DateTimeKind.Utc
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is Account account)
            {
                account.UpdatedAt = DateTime.UtcNow;
                
                // Ensure all DateTime properties are UTC
                if (account.CreatedAt.Kind != DateTimeKind.Utc)
                    account.CreatedAt = DateTime.SpecifyKind(account.CreatedAt, DateTimeKind.Utc);
                if (account.UpdatedAt.Kind != DateTimeKind.Utc)
                    account.UpdatedAt = DateTime.SpecifyKind(account.UpdatedAt, DateTimeKind.Utc);
                if (account.Dob.HasValue && account.Dob.Value.Kind != DateTimeKind.Utc)
                    account.Dob = DateTime.SpecifyKind(account.Dob.Value, DateTimeKind.Utc);
            }
            else if (entry.Entity is Role role)
            {
                role.UpdatedAt = DateTime.UtcNow;
                
                // Ensure all DateTime properties are UTC
                if (role.CreatedAt.Kind != DateTimeKind.Utc)
                    role.CreatedAt = DateTime.SpecifyKind(role.CreatedAt, DateTimeKind.Utc);
                if (role.UpdatedAt.Kind != DateTimeKind.Utc)
                    role.UpdatedAt = DateTime.SpecifyKind(role.UpdatedAt, DateTimeKind.Utc);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
