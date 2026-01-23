namespace PaymentService.Domain.Enums;

/// <summary>
/// Trạng thái giao dịch ví
/// </summary>
public enum WalletTransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Reversed = 3
}
