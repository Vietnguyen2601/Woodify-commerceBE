using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Entity WalletTransaction - giao dịch ví
/// </summary>
public class WalletTransaction
{
    public Guid WalletTxId { get; set; }

    public Guid WalletId { get; set; }

    public WalletTransactionType TxType { get; set; }

    /// <summary>
    /// Số tiền giao dịch (VND)
    /// </summary>
    public long AmountVnd { get; set; }

    public long? BalanceBeforeVnd { get; set; }

    public long? BalanceAfterVnd { get; set; }

    /// <summary>
    /// FK to Orders service (optional)
    /// </summary>
    public Guid? RelatedOrderId { get; set; }

    /// <summary>
    /// Shop liên quan (seller payout / reversal).
    /// </summary>
    public Guid? RelatedShopId { get; set; }

    /// <summary>
    /// FK to Payment (optional)
    /// </summary>
    public Guid? RelatedPaymentId { get; set; }

    /// <summary>
    /// Loại nghiệp vụ ledger: ORDER_NET_CREDIT, ORDER_NET_REVERSAL, WITHDRAWAL_PAYOUT, ...
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Khóa idempotent — mỗi ghi có/ghi nợ nghiệp vụ chỉ một bản ghi.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public string? Note { get; set; }

    // Navigation
    public Wallet? Wallet { get; set; }
}
