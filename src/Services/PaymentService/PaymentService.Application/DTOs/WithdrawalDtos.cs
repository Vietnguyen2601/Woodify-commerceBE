using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

public class CreateSellerWithdrawalRequest
{
    public Guid SellerAccountId { get; set; }
    public Guid ShopId { get; set; }
    public long AmountVnd { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountHolder { get; set; } = string.Empty;
}

public class AdminReviewWithdrawalRequest
{
    public Guid AdminAccountId { get; set; }
    public string? Note { get; set; }
}

public class WithdrawalTicketResponse
{
    public Guid TicketId { get; set; }
    public Guid SellerAccountId { get; set; }
    public Guid ShopId { get; set; }
    public long AmountVnd { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountHolder { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ReviewedByAccountId { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class WithdrawalTicketListResponse
{
    public List<WithdrawalTicketResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
