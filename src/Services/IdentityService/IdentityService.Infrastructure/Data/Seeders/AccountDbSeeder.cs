using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder cho IdentityService Database
/// Khởi tạo dữ liệu Roles và Accounts mặc định
/// </summary>
public static class AccountDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as IdentityService.Infrastructure.Data.Context.AccountDbContext;
        if (dbContext == null)
            return;

        // Seed Roles nếu chưa có
        if (!dbContext.Roles.Any())
        {
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
                    RoleName = "Support",
                    Description = "Handles customer inquiries, tickets, and troubleshooting.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    RoleId = Guid.NewGuid(),
                    RoleName = "Staff",
                    Description = "Internal staff with limited management and operational permissions.",
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

        // Seed Customer Accounts nếu chưa có
        if (!dbContext.Accounts.Any())
        {
            var customerRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "Customer");

            var accounts = new List<Account>
            {
                new()
                {
                    AccountId = Guid.NewGuid(),
                    Username = "customer01",
                    Password = "password7",
                    Email = "customer01@gmail.com",
                    Name = "Lê Văn Customer",
                    PhoneNumber = "0901000007",
                    Address = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                    Dob = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Male",
                    RoleId = customerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    AccountId = Guid.NewGuid(),
                    Username = "customer02",
                    Password = "password8",
                    Email = "customer02@gmail.com",
                    Name = "Phạm Thị Customer",
                    PhoneNumber = "0901000008",
                    Address = "456 Lê Lợi, Quận 10, TP.HCM",
                    Dob = new DateTime(1992, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Female",
                    RoleId = customerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    AccountId = Guid.NewGuid(),
                    Username = "customer03",
                    Password = "password9",
                    Email = "customer03@gmail.com",
                    Name = "Trần Văn Khách",
                    PhoneNumber = "0901000009",
                    Address = "789 Trần Hưng Đạo, Quận 5, TP.HCM",
                    Dob = new DateTime(1988, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Male",
                    RoleId = customerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    AccountId = Guid.NewGuid(),
                    Username = "customer04",
                    Password = "password10",
                    Email = "customer04@gmail.com",
                    Name = "Hoàng Thị Mua Hàng",
                    PhoneNumber = "0901000010",
                    Address = "321 Cách Mạng Tháng Tám, Quận 3, TP.HCM",
                    Dob = new DateTime(1995, 11, 7, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Female",
                    RoleId = customerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new()
                {
                    AccountId = Guid.NewGuid(),
                    Username = "customer05",
                    Password = "password11",
                    Email = "customer05@gmail.com",
                    Name = "Ngô Quốc Huy",
                    PhoneNumber = "0901000011",
                    Address = "654 Võ Văn Tần, Quận 3, TP.HCM",
                    Dob = new DateTime(1993, 6, 28, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Male",
                    RoleId = customerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            dbContext.Accounts.AddRange(accounts);
            await dbContext.SaveChangesAsync();
        }
    }
}
