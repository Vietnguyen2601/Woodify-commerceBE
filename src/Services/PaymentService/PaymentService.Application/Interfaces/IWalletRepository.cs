using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

/// <summary>
/// Interface cho Wallet Repository
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Lấy Wallet theo ID
    /// </summary>
    Task<Wallet?> GetByIdAsync(Guid walletId);

    /// <summary>
    /// Lấy Wallet theo AccountId
    /// </summary>
    Task<Wallet?> GetByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Tạo mới Wallet
    /// </summary>
    Task<Wallet> CreateAsync(Wallet wallet);

    /// <summary>
    /// Cập nhật Wallet
    /// </summary>
    Task<Wallet> UpdateAsync(Wallet wallet);

    /// <summary>
    /// Lấy danh sách giao dịch của Wallet
    /// </summary>
    Task<(IEnumerable<WalletTransaction> Items, int TotalCount)> GetTransactionsAsync(
        Guid walletId, int page, int pageSize);

    /// <summary>
    /// Thêm giao dịch mới
    /// </summary>
    Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction);
}
