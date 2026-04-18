namespace PaymentService.Domain.Enums;

/// <summary>
/// Buyer = ví duy nhất đang dùng (nạp, thanh toán, doanh thu shop, rút). Seller = legacy, không tạo mới.
/// </summary>
public enum WalletKind
{
    Buyer = 0,
    Seller = 1
}
