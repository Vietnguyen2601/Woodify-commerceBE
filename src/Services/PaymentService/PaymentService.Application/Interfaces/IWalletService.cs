using PaymentService.Application.DTOs;
using Shared.Results;

namespace PaymentService.Application.Interfaces;

/// <summary>
/// Interface cho Wallet Service
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Tạo ví mới cho Account
    /// </summary>
    Task<ServiceResult<WalletResponse>> CreateWalletAsync(CreateWalletRequest request);

    /// <summary>
    /// Lấy thông tin ví theo ID
    /// </summary>
    Task<ServiceResult<WalletResponse>> GetWalletAsync(Guid walletId);

    /// <summary>
    /// Lấy thông tin ví theo AccountId
    /// </summary>
    Task<ServiceResult<WalletResponse>> GetWalletByAccountAsync(Guid accountId);

    /// <summary>
    /// Nạp tiền vào ví
    /// </summary>
    Task<ServiceResult<WalletTransactionResult>> CreditAsync(Guid walletId, CreditWalletRequest request);

    /// <summary>
    /// Trừ tiền từ ví
    /// </summary>
    Task<ServiceResult<WalletTransactionResult>> DebitAsync(Guid walletId, DebitWalletRequest request);

    /// <summary>
    /// Lấy lịch sử giao dịch
    /// </summary>
    Task<ServiceResult<WalletTransactionListResponse>> GetTransactionsAsync(
        Guid walletId, int page = 1, int pageSize = 20);

    Task<ServiceResult<WalletTopUpResponse>> TopUpAsync(WalletTopUpRequest request);
}
