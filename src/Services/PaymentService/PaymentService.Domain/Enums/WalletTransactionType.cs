namespace PaymentService.Domain.Enums;

/// <summary>
/// Loại giao dịch ví
/// </summary>
public enum WalletTransactionType
{
    Credit = 0,  // Nạp tiền
    Debit = 1,   // Rút tiền
    Hold = 2,    // Tạm giữ
    Release = 3  // Giải phóng
}
