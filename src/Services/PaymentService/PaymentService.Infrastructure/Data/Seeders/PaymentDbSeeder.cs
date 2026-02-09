using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Infrastructure.Data.Seeders;

/// <summary>
/// Seeder cho PaymentService Database
/// Khởi tạo dữ liệu Wallets mặc định
/// </summary>
public static class PaymentDbSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbContext = context as PaymentService.Infrastructure.Data.PaymentDbContext;
        if (dbContext == null)
            return;

        // Seed Wallets nếu chưa có
        if (!dbContext.Wallets.Any())
        {
            var wallets = new List<Wallet>
            {
                new()
                {
                    WalletId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    BalanceCents = 0,
                    Currency = "VND",
                    Status = WalletStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    WalletId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    BalanceCents = 0,
                    Currency = "VND",
                    Status = WalletStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    WalletId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    BalanceCents = 0,
                    Currency = "VND",
                    Status = WalletStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    WalletId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    BalanceCents = 0,
                    Currency = "VND",
                    Status = WalletStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    WalletId = Guid.NewGuid(),
                    AccountId = Guid.NewGuid(),
                    BalanceCents = 0,
                    Currency = "VND",
                    Status = WalletStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            dbContext.Wallets.AddRange(wallets);
            await dbContext.SaveChangesAsync();
        }

        // Payments and WalletTransactions không cần seed vì được tạo bởi user actions
    }
}
