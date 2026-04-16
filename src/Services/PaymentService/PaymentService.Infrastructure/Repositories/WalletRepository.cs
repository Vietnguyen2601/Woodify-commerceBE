using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Implementation của Wallet Repository
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _context;

    public WalletRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByIdAsync(Guid walletId)
    {
        return await _context.Wallets.FindAsync(walletId);
    }

    public async Task<Wallet?> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.AccountId == accountId);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        wallet.WalletId = Guid.NewGuid();
        wallet.CreatedAt = DateTime.UtcNow;

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }

    public async Task<(IEnumerable<WalletTransaction> Items, int TotalCount)> GetTransactionsAsync(
        Guid walletId, int page, int pageSize)
    {
        var query = _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction)
    {
        transaction.WalletTxId = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;

        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }
}
