using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Constants;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;
using Shared.Events;

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
            .FirstOrDefaultAsync(w => w.AccountId == accountId && w.WalletKind == WalletKind.Buyer);
    }

    public async Task<Wallet?> GetByAccountIdAndKindAsync(Guid accountId, WalletKind kind)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.AccountId == accountId && w.WalletKind == kind);
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

    /// <summary>
    /// Ví duy nhất của user (Buyer) — dùng chung cho nạp/thanh toán và doanh thu seller.
    /// </summary>
    private async Task<Wallet> GetOrCreateBuyerWalletAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(
            w => w.AccountId == accountId && w.WalletKind == WalletKind.Buyer,
            cancellationToken);
        if (wallet != null)
            return wallet;

        wallet = new Wallet
        {
            WalletId = Guid.NewGuid(),
            AccountId = accountId,
            WalletKind = WalletKind.Buyer,
            Currency = "VND",
            Status = WalletStatus.Active,
            BalanceVnd = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync(cancellationToken);
        return wallet;
    }

    public async Task ApplySellerOrderNetCreditAsync(OrderSellerNetEligibleEvent evt, CancellationToken cancellationToken = default)
    {
        if (evt.NetAmountVnd <= 0 || string.IsNullOrEmpty(evt.IdempotencyKey))
            return;

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            var exists = await _context.WalletTransactions
                .AnyAsync(t => t.IdempotencyKey == evt.IdempotencyKey, cancellationToken);
            if (exists)
            {
                await tx.CommitAsync(cancellationToken);
                return;
            }

            var wallet = await GetOrCreateBuyerWalletAsync(evt.SellerAccountId, cancellationToken);

            var before = wallet.BalanceVnd;
            wallet.BalanceVnd += evt.NetAmountVnd;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletTxId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TxType = WalletTransactionType.Credit,
                AmountVnd = evt.NetAmountVnd,
                BalanceBeforeVnd = before,
                BalanceAfterVnd = wallet.BalanceVnd,
                RelatedOrderId = evt.OrderId,
                RelatedShopId = evt.ShopId,
                ReferenceType = WalletTxReferenceTypes.OrderNetCredit,
                IdempotencyKey = evt.IdempotencyKey,
                Status = WalletTransactionStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                Note = $"Seller net order {evt.OrderId}"
            });

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        });
    }

    public async Task ApplySellerOrderNetReversalAsync(OrderSellerNetReversedEvent evt, CancellationToken cancellationToken = default)
    {
        if (evt.NetAmountVnd <= 0 || string.IsNullOrEmpty(evt.IdempotencyKey))
            return;

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            var exists = await _context.WalletTransactions
                .AnyAsync(t => t.IdempotencyKey == evt.IdempotencyKey, cancellationToken);
            if (exists)
            {
                await tx.CommitAsync(cancellationToken);
                return;
            }

            var wallet = await GetOrCreateBuyerWalletAsync(evt.SellerAccountId, cancellationToken);

            var before = wallet.BalanceVnd;
            wallet.BalanceVnd -= evt.NetAmountVnd;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletTxId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TxType = WalletTransactionType.Debit,
                AmountVnd = evt.NetAmountVnd,
                BalanceBeforeVnd = before,
                BalanceAfterVnd = wallet.BalanceVnd,
                RelatedOrderId = evt.OrderId,
                RelatedShopId = evt.ShopId,
                ReferenceType = WalletTxReferenceTypes.OrderNetReversal,
                IdempotencyKey = evt.IdempotencyKey,
                Status = WalletTransactionStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                Note = $"Reversal order {evt.OrderId}"
            });

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        });
    }

    public async Task<(bool Success, string Error)> ApplyWithdrawalPayoutDebitAsync(
        Guid sellerAccountId,
        long amountVnd,
        Guid ticketId,
        Guid shopId,
        CancellationToken cancellationToken = default)
    {
        if (amountVnd <= 0)
            return (false, "Invalid amount");

        var idempotencyKey = $"withdrawal_payout:{ticketId}";
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);
            if (await _context.WalletTransactions.AnyAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken))
            {
                await tx.CommitAsync(cancellationToken);
                return (true, string.Empty);
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(
                w => w.AccountId == sellerAccountId && w.WalletKind == WalletKind.Buyer,
                cancellationToken);

            if (wallet == null)
            {
                await tx.RollbackAsync(cancellationToken);
                return (false, "Chưa có ví tài khoản (Buyer)");
            }

            if (wallet.BalanceVnd < amountVnd)
            {
                await tx.RollbackAsync(cancellationToken);
                return (false, $"Số dư không đủ. Hiện có {wallet.BalanceVnd}, cần {amountVnd}");
            }

            var before = wallet.BalanceVnd;
            wallet.BalanceVnd -= amountVnd;
            wallet.UpdatedAt = DateTime.UtcNow;

            _context.WalletTransactions.Add(new WalletTransaction
            {
                WalletTxId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                TxType = WalletTransactionType.Debit,
                AmountVnd = amountVnd,
                BalanceBeforeVnd = before,
                BalanceAfterVnd = wallet.BalanceVnd,
                RelatedShopId = shopId,
                ReferenceType = WalletTxReferenceTypes.WithdrawalPayout,
                IdempotencyKey = idempotencyKey,
                Status = WalletTransactionStatus.Completed,
                CompletedAt = DateTime.UtcNow,
                Note = $"Withdrawal ticket {ticketId}"
            });

            await _context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return (true, string.Empty);
        });
    }
}
