using Microsoft.EntityFrameworkCore;
using ShopService.Domain.Entities;

namespace ShopService.Infrastructure.Data.Context;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }

    public DbSet<Shop> Shops { get; set; }
    public DbSet<ShopFollower> ShopFollowers { get; set; }

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
