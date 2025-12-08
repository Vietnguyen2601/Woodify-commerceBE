using AccountService.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories.DBContext;

/// <summary>
/// DbContext cho Account Service
/// Quản lý kết nối database và mapping entities
/// </summary>
public class AccountDbContext : DbContext
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }

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
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
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
        // Tự động cập nhật UpdatedAt khi save
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Account account)
                account.UpdatedAt = DateTime.UtcNow;
            else if (entry.Entity is Role role)
                role.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
