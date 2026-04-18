using PaymentService.Application.DTOs;
using Shared.Results;

namespace PaymentService.Application.Interfaces;

public interface ISellerWalletReadService
{
    Task<ServiceResult<WalletResponse>> GetOrCreateSellerWalletAsync(Guid accountId);
    Task<ServiceResult<WalletTransactionListResponse>> GetSellerTransactionsAsync(
        Guid accountId, int page = 1, int pageSize = 20);
}
