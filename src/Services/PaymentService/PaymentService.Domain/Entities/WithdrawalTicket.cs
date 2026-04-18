using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

/// <summary>
/// Ticket rút tiền từ ví seller — admin duyệt và xác nhận CK tay.
/// </summary>
public class WithdrawalTicket
{
    public Guid TicketId { get; set; }

    /// <summary>Chủ shop (cùng AccountId với ví seller).</summary>
    public Guid SellerAccountId { get; set; }

    public Guid ShopId { get; set; }

    public long AmountVnd { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountHolder { get; set; } = string.Empty;

    public WithdrawalTicketStatus Status { get; set; } = WithdrawalTicketStatus.Pending;

    public Guid? ReviewedByAccountId { get; set; }
    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
