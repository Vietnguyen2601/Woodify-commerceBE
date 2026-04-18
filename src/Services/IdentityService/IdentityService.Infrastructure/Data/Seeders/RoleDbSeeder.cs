using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds default roles (Admin, Seller, Customer) when the database is empty.
/// </summary>
public static class RoleDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as IdentityService.Infrastructure.Data.Context.AccountDbContext;
        if (dbContext == null)
            return;

        if (dbContext.Roles.Any())
            return;

        var roles = new List<Role>
        {
            new()
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Admin",
                Description = "Full access: manage users, roles, system settings and data.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Seller",
                Description = "Seller role: manage product listings, inventory and orders.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new()
            {
                RoleId = Guid.NewGuid(),
                RoleName = "Customer",
                Description = "End user: browse products, place orders and manage their account.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        dbContext.Roles.AddRange(roles);
        await dbContext.SaveChangesAsync();
    }
}
