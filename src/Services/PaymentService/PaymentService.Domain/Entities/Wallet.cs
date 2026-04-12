using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Entity Wallet - ví điện tử của người dùng
/// </summary>
public class Wallet
{
    public Guid WalletId { get; set; }

    /// <summary>
    /// FK to Accounts service
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Số dư (VND)
    /// </summary>
    public long BalanceVnd { get; set; } = 0;

    public string Currency { get; set; } = "VND";

    public WalletStatus Status { get; set; } = WalletStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
