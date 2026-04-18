using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Services;

public class SellerWalletReadService : ISellerWalletReadService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger<SellerWalletReadService> _logger;

    public SellerWalletReadService(IWalletRepository walletRepository, ILogger<SellerWalletReadService> logger)
    {
        _walletRepository = walletRepository;
        _logger = logger;
    }

    /// <summary>
    /// Trả về ví chính của account (Buyer) — cùng ví lúc đăng ký; doanh thu seller cộng vào đây.
    /// </summary>
    public async Task<ServiceResult<WalletResponse>> GetOrCreateSellerWalletAsync(Guid accountId)
    {
        try
        {
            var wallet = await _walletRepository.GetByAccountIdAsync(accountId);
            if (wallet == null)
            {
                wallet = await _walletRepository.CreateAsync(new Wallet
                {
                    AccountId = accountId,
                    WalletKind = WalletKind.Buyer,
                    Currency = "VND",
                    BalanceVnd = 0,
                    Status = WalletStatus.Active
                });
                _logger.LogInformation("Created primary wallet for account {AccountId} (seller UI)", accountId);
            }

            return ServiceResult<WalletResponse>.Success(Map(wallet));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrCreateSellerWalletAsync {AccountId}", accountId);
            return ServiceResult<WalletResponse>.InternalServerError(ex.Message);
        }
    }

    public async Task<ServiceResult<WalletTransactionListResponse>> GetSellerTransactionsAsync(
        Guid accountId, int page = 1, int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var wallet = await _walletRepository.GetByAccountIdAsync(accountId);
            if (wallet == null)
            {
                return ServiceResult<WalletTransactionListResponse>.Success(new WalletTransactionListResponse
                {
                    Items = new List<WalletTransactionResponse>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                });
            }

            var (items, total) = await _walletRepository.GetTransactionsAsync(wallet.WalletId, page, pageSize);
            return ServiceResult<WalletTransactionListResponse>.Success(new WalletTransactionListResponse
            {
                Items = items.Select(MapTx).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSellerTransactionsAsync {AccountId}", accountId);
            return ServiceResult<WalletTransactionListResponse>.InternalServerError(ex.Message);
        }
    }

    private static WalletResponse Map(Wallet w) => new()
    {
        WalletId = w.WalletId,
        AccountId = w.AccountId,
        Balance = w.BalanceVnd,
        Currency = w.Currency,
        Status = w.Status.ToString(),
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt,
        WalletKind = w.WalletKind.ToString()
    };

    private static WalletTransactionResponse MapTx(Domain.Entities.WalletTransaction tx) => new()
    {
        TransactionId = tx.WalletTxId,
        WalletId = tx.WalletId,
        TransactionType = tx.TxType.ToString(),
        Amount = tx.AmountVnd,
        BalanceBefore = tx.BalanceBeforeVnd,
        BalanceAfter = tx.BalanceAfterVnd,
        RelatedOrderId = tx.RelatedOrderId,
        RelatedPaymentId = tx.RelatedPaymentId,
        RelatedShopId = tx.RelatedShopId,
        ReferenceType = tx.ReferenceType,
        Status = tx.Status.ToString(),
        CreatedAt = tx.CreatedAt,
        CompletedAt = tx.CompletedAt,
        Note = tx.Note
    };
}
