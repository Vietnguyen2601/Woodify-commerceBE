using Microsoft.EntityFrameworkCore;
using ShopService.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace ShopService.Infrastructure.Data.Context;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopFollower> ShopFollowers { get; set; }

     private static string? GetConnectionString(string connectionStringName)
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
        string? envConnectionString = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionStringName}");
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

    private static string? FindEnvFile()
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
            optionsBuilder.UseNpgsql(GetConnectionString("ShopService"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.ToTable("shops");
            entity.HasKey(e => e.ShopId);
            
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.OwnerAccountId).HasColumnName("owner_account_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.FollowerCount).HasColumnName("follower_count");
            
            // Index
            entity.HasIndex(e => e.OwnerAccountId);
            entity.HasIndex(e => e.Name);
            
            // Relationship
            entity.HasMany(s => s.Followers)
                .WithOne(f => f.Shop)
                .HasForeignKey(f => f.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShopFollower>(entity =>
        {
            entity.ToTable("shop_followers");
            entity.HasKey(e => e.ShopFollowerId);
            
            entity.Property(e => e.ShopFollowerId).HasColumnName("shop_follower_id");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat");
        });
    }
}
