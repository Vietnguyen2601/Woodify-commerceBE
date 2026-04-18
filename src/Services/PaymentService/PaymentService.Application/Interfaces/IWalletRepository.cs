using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Events;

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
    /// Ví buyer (nạp / thanh toán) — mặc định cho AccountId.
    /// </summary>
    Task<Wallet?> GetByAccountIdAsync(Guid accountId);

    Task<Wallet?> GetByAccountIdAndKindAsync(Guid accountId, WalletKind kind);

    /// <summary>
    /// Tạo mới Wallet
    /// </summary>
    Task<Wallet> CreateAsync(Wallet wallet);

    /// <summary>
    /// Ghi có net seller theo đơn (idempotent).
    /// </summary>
    Task ApplySellerOrderNetCreditAsync(OrderSellerNetEligibleEvent evt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hoàn tác ghi có khi hủy/hoàn đơn sau COMPLETED (idempotent). Cho phép số dư âm nếu seller đã rút.
    /// </summary>
    Task ApplySellerOrderNetReversalAsync(OrderSellerNetReversedEvent evt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trừ ví seller khi admin xác nhận đã chuyển khoản rút tiền (idempotent theo ticket).
    /// </summary>
    Task<(bool Success, string Error)> ApplyWithdrawalPayoutDebitAsync(
        Guid sellerAccountId,
        long amountVnd,
        Guid ticketId,
        Guid shopId,
        CancellationToken cancellationToken = default);

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
