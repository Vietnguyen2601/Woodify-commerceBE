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

        // Seed Seller Accounts nếu chưa có
        if (!dbContext.Accounts.Any())
        {
            var sellerRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "Seller");
            var customerRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "Customer");

            var accounts = new List<Account>
            {
                // Seller 01
                new()
                {
                    AccountId = new Guid("a7b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d"),
                    Username = "seller01",
                    Password = "seller_pass_01",
                    Email = "seller01@woodify.com",
                    Name = "Nguyễn Văn Seller",
                    PhoneNumber = "0901111111",
                    Address = "100 Pasteur, Quận 1, TP.HCM",
                    Dob = new DateTime(1985, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Male",
                    RoleId = sellerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                    IsActive = true
                },
                // Seller 02
                new()
                {
                    AccountId = new Guid("b8c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e"),
                    Username = "seller02",
                    Password = "seller_pass_02",
                    Email = "seller02@woodify.com",
                    Name = "Trần Thị Seller",
                    PhoneNumber = "0901222222",
                    Address = "200 Đinh Tiên Hoàng, Quận Bình Thạnh, TP.HCM",
                    Dob = new DateTime(1987, 7, 22, 0, 0, 0, DateTimeKind.Utc),
                    Gender = "Female",
                    RoleId = sellerRole?.RoleId,
                    CreatedAt = DateTime.UtcNow.AddDays(-50),
                    UpdatedAt = DateTime.UtcNow.AddDays(-50),
                    IsActive = true
                },
                // Customer 01
                new()
                {
                    AccountId = new Guid("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"),
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
                    AccountId = new Guid("d2e3f4a5-b6c7-4d8e-9f0a-1b2c3d4e5f6a"),
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
                    AccountId = new Guid("e3f4a5b6-c7d8-4e9f-0a1b-2c3d4e5f6a7b"),
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
