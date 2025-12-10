using Microsoft.EntityFrameworkCore;
using ShopService.Repositories.Models;

namespace ShopService.Repositories.DBContext;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId);
            entity.Property(e => e.ShopName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            
            // Index cho tìm kiếm nhanh
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ShopName);
        });
    }
}
