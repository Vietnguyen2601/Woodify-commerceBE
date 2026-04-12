using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Services;

/// <summary>
/// Wallet Service - Business logic cho ví điện tử
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly WalletTopUpService _walletTopUpService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepository,
        WalletTopUpService walletTopUpService,
        ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _walletTopUpService = walletTopUpService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo ví mới cho Account
    /// </summary>
    public async Task<ServiceResult<WalletResponse>> CreateWalletAsync(CreateWalletRequest request)
    {
        try
        {
            _logger.LogInformation("Creating wallet for accountId: {AccountId}", request.AccountId);

            // Check nếu account đã có ví
            var existingWallet = await _walletRepository.GetByAccountIdAsync(request.AccountId);
            if (existingWallet != null)
            {
                return ServiceResult<WalletResponse>.Conflict(
                    $"Account {request.AccountId} đã có ví");
            }

            var wallet = new Wallet
            {
                AccountId = request.AccountId,
                Currency = request.Currency,
                BalanceVnd = 0,
                Status = WalletStatus.Active
            };

            var created = await _walletRepository.CreateAsync(wallet);

            _logger.LogInformation("Wallet created successfully. WalletId: {WalletId}", created.WalletId);

            return ServiceResult<WalletResponse>.Created(MapToResponse(created), "Tạo ví thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet for accountId: {AccountId}", request.AccountId);
            return ServiceResult<WalletResponse>.InternalServerError($"Lỗi tạo ví: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy thông tin ví theo ID
    /// </summary>
    public async Task<ServiceResult<WalletResponse>> GetWalletAsync(Guid walletId)
    {
        try
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResult<WalletResponse>.NotFound("Wallet not found");
            }

            return ServiceResult<WalletResponse>.Success(MapToResponse(wallet));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet: {WalletId}", walletId);
            return ServiceResult<WalletResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy thông tin ví theo AccountId
    /// </summary>
    public async Task<ServiceResult<WalletResponse>> GetWalletByAccountAsync(Guid accountId)
    {
        try
        {
            var wallet = await _walletRepository.GetByAccountIdAsync(accountId);
            if (wallet == null)
            {
                return ServiceResult<WalletResponse>.NotFound(
                    $"Không tìm thấy ví cho account: {accountId}");
            }

            return ServiceResult<WalletResponse>.Success(MapToResponse(wallet));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet by accountId: {AccountId}", accountId);
            return ServiceResult<WalletResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Nạp tiền vào ví
    /// </summary>
    public async Task<ServiceResult<WalletTransactionResult>> CreditAsync(
        Guid walletId, CreditWalletRequest request)
    {
        try
        {
            _logger.LogInformation("Credit wallet {WalletId} with amount: {Amount}",
                walletId, request.Amount);

            // Validate
            if (request.Amount <= 0)
            {
                return ServiceResult<WalletTransactionResult>.BadRequest("Số tiền phải lớn hơn 0");
            }

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResult<WalletTransactionResult>.NotFound("Wallet not found");
            }

            if (wallet.Status != WalletStatus.Active)
            {
                return ServiceResult<WalletTransactionResult>.BadRequest(
                    $"Ví đang ở trạng thái {wallet.Status}, không thể thực hiện giao dịch");
            }

            // Lưu balance trước giao dịch
            var balanceBefore = wallet.BalanceVnd;

            // Tạo transaction
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                TxType = WalletTransactionType.Credit,
                AmountVnd = request.Amount,
                BalanceBeforeVnd = balanceBefore,
                BalanceAfterVnd = balanceBefore + request.Amount,
                RelatedPaymentId = request.RelatedPaymentId,
                RelatedOrderId = request.RelatedOrderId,
                Note = request.Note,
                Status = WalletTransactionStatus.Completed,
                CompletedAt = DateTime.UtcNow
            };

            // Cập nhật balance
            wallet.BalanceVnd += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Save
            await _walletRepository.AddTransactionAsync(transaction);
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation(
                "Credit successful. WalletId: {WalletId}, Amount: {Amount}, NewBalance: {Balance}",
                walletId, request.Amount, wallet.BalanceVnd);

            return ServiceResult<WalletTransactionResult>.Success(new WalletTransactionResult
            {
                Success = true,
                TransactionId = transaction.WalletTxId,
                NewBalance = wallet.BalanceVnd,
                Message = $"Nạp tiền thành công. Số dư mới: {wallet.BalanceVnd:N0} VND"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crediting wallet: {WalletId}", walletId);
            return ServiceResult<WalletTransactionResult>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Trừ tiền từ ví
    /// </summary>
    public async Task<ServiceResult<WalletTransactionResult>> DebitAsync(
        Guid walletId, DebitWalletRequest request)
    {
        try
        {
            _logger.LogInformation("Debit wallet {WalletId} with amount: {Amount}",
                walletId, request.Amount);

            // Validate
            if (request.Amount <= 0)
            {
                return ServiceResult<WalletTransactionResult>.BadRequest("Số tiền phải lớn hơn 0");
            }

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResult<WalletTransactionResult>.NotFound("Wallet not found");
            }

            if (wallet.Status != WalletStatus.Active)
            {
                return ServiceResult<WalletTransactionResult>.BadRequest(
                    $"Ví đang ở trạng thái {wallet.Status}, không thể thực hiện giao dịch");
            }

            // Check đủ số dư
            if (wallet.BalanceVnd < request.Amount)
            {
                return ServiceResult<WalletTransactionResult>.BadRequest(
                    $"Số dư không đủ. Hiện có: {wallet.BalanceVnd:N0} VND, cần: {request.Amount:N0} VND");
            }

            // Lưu balance trước giao dịch
            var balanceBefore = wallet.BalanceVnd;

            // Tạo transaction
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                TxType = WalletTransactionType.Debit,
                AmountVnd = request.Amount,
                BalanceBeforeVnd = balanceBefore,
                BalanceAfterVnd = balanceBefore - request.Amount,
                RelatedPaymentId = request.RelatedPaymentId,
                RelatedOrderId = request.RelatedOrderId,
                Note = request.Note,
                Status = WalletTransactionStatus.Completed,
                CompletedAt = DateTime.UtcNow
            };

            // Cập nhật balance
            wallet.BalanceVnd -= request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Save
            await _walletRepository.AddTransactionAsync(transaction);
            await _walletRepository.UpdateAsync(wallet);

            _logger.LogInformation(
                "Debit successful. WalletId: {WalletId}, Amount: {Amount}, NewBalance: {Balance}",
                walletId, request.Amount, wallet.BalanceVnd);

            return ServiceResult<WalletTransactionResult>.Success(new WalletTransactionResult
            {
                Success = true,
                TransactionId = transaction.WalletTxId,
                NewBalance = wallet.BalanceVnd,
                Message = $"Trừ tiền thành công. Số dư mới: {wallet.BalanceVnd:N0} VND"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debiting wallet: {WalletId}", walletId);
            return ServiceResult<WalletTransactionResult>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy lịch sử giao dịch
    /// </summary>
    public async Task<ServiceResult<WalletTransactionListResponse>> GetTransactionsAsync(
        Guid walletId, int page = 1, int pageSize = 20)
    {
        try
        {
            // Validate
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
            {
                return ServiceResult<WalletTransactionListResponse>.NotFound("Wallet not found");
            }

            var (items, totalCount) = await _walletRepository.GetTransactionsAsync(
                walletId, page, pageSize);

            var response = new WalletTransactionListResponse
            {
                Items = items.Select(MapToTransactionResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return ServiceResult<WalletTransactionListResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for wallet: {WalletId}", walletId);
            return ServiceResult<WalletTransactionListResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    #region Private Methods

    private static WalletResponse MapToResponse(Wallet wallet)
    {
        return new WalletResponse
        {
            WalletId = wallet.WalletId,
            AccountId = wallet.AccountId,
            Balance = wallet.BalanceVnd,
            Currency = wallet.Currency,
            Status = wallet.Status.ToString(),
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    private static WalletTransactionResponse MapToTransactionResponse(WalletTransaction tx)
    {
        return new WalletTransactionResponse
        {
            TransactionId = tx.WalletTxId,
            WalletId = tx.WalletId,
            TransactionType = tx.TxType.ToString(),
            Amount = tx.AmountVnd,
            BalanceBefore = tx.BalanceBeforeVnd,
            BalanceAfter = tx.BalanceAfterVnd,
            RelatedOrderId = tx.RelatedOrderId,
            RelatedPaymentId = tx.RelatedPaymentId,
            Status = tx.Status.ToString(),
            CreatedAt = tx.CreatedAt,
            CompletedAt = tx.CompletedAt,
            Note = tx.Note
        };
    }

    #endregion

    public Task<ServiceResult<WalletTopUpResponse>> TopUpAsync(WalletTopUpRequest request)
        => _walletTopUpService.TopUpAsync(request);
}
