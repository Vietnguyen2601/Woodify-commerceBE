using Microsoft.Extensions.Logging;
using PaymentService.Application.Constants;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Services;

public class WithdrawalTicketService : IWithdrawalTicketService
{
    private readonly IWithdrawalTicketRepository _tickets;
    private readonly IWalletRepository _wallets;
    private readonly ILogger<WithdrawalTicketService> _logger;

    public WithdrawalTicketService(
        IWithdrawalTicketRepository tickets,
        IWalletRepository wallets,
        ILogger<WithdrawalTicketService> logger)
    {
        _tickets = tickets;
        _wallets = wallets;
        _logger = logger;
    }

    public async Task<ServiceResult<WithdrawalTicketResponse>> CreateAsync(CreateSellerWithdrawalRequest request)
    {
        if (request.AmountVnd < SellerWalletConstants.MinWithdrawalVnd)
        {
            return ServiceResult<WithdrawalTicketResponse>.BadRequest(
                $"Số tiền rút tối thiểu {SellerWalletConstants.MinWithdrawalVnd:N0} VND");
        }

        if (string.IsNullOrWhiteSpace(request.BankName) ||
            string.IsNullOrWhiteSpace(request.BankAccountNumber) ||
            string.IsNullOrWhiteSpace(request.BankAccountHolder))
        {
            return ServiceResult<WithdrawalTicketResponse>.BadRequest("Thiếu thông tin ngân hàng");
        }

        var wallet = await _wallets.GetByAccountIdAsync(request.SellerAccountId);
        if (wallet == null || wallet.BalanceVnd < request.AmountVnd)
        {
            return ServiceResult<WithdrawalTicketResponse>.BadRequest(
                "Số dư ví không đủ");
        }

        var pending = await _tickets.ListAsync(WithdrawalTicketStatus.Pending, 1, 50);
        if (pending.Items.Any(t => t.SellerAccountId == request.SellerAccountId))
        {
            return ServiceResult<WithdrawalTicketResponse>.Conflict(
                "Đang có yêu cầu rút tiền chờ duyệt");
        }

        var ticket = new WithdrawalTicket
        {
            SellerAccountId = request.SellerAccountId,
            ShopId = request.ShopId,
            AmountVnd = request.AmountVnd,
            BankName = request.BankName.Trim(),
            BankAccountNumber = request.BankAccountNumber.Trim(),
            BankAccountHolder = request.BankAccountHolder.Trim(),
            Status = WithdrawalTicketStatus.Pending
        };

        var created = await _tickets.AddAsync(ticket);
        _logger.LogInformation("Withdrawal ticket {TicketId} created for seller {AccountId}", created.TicketId, request.SellerAccountId);
        return ServiceResult<WithdrawalTicketResponse>.Success(Map(created), "Đã gửi yêu cầu rút tiền");
    }

    public async Task<ServiceResult<WithdrawalTicketListResponse>> ListAdminAsync(
        WithdrawalTicketStatus? status, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (items, total) = await _tickets.ListAsync(status, page, pageSize);
        return ServiceResult<WithdrawalTicketListResponse>.Success(new WithdrawalTicketListResponse
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ServiceResult<WithdrawalTicketListResponse>> ListAllAdminAsync(
        WithdrawalTicketStatus? status,
        int maxRows = 10_000)
    {
        var (items, totalInDb) = await _tickets.GetAllAsync(status, maxRows);
        var list = items.Select(Map).ToList();
        return ServiceResult<WithdrawalTicketListResponse>.Success(new WithdrawalTicketListResponse
        {
            Items = list,
            TotalCount = totalInDb,
            Page = 1,
            PageSize = list.Count
        });
    }

    public async Task<ServiceResult<WithdrawalTicketResponse>> ApproveAsync(Guid ticketId, AdminReviewWithdrawalRequest request)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ServiceResult<WithdrawalTicketResponse>.NotFound("Ticket không tồn tại");
        if (ticket.Status != WithdrawalTicketStatus.Pending)
            return ServiceResult<WithdrawalTicketResponse>.BadRequest("Trạng thái ticket không hợp lệ");

        var (ok, err) = await _wallets.ApplyWithdrawalPayoutDebitAsync(
            ticket.SellerAccountId,
            ticket.AmountVnd,
            ticket.TicketId,
            ticket.ShopId);

        if (!ok)
        {
            _logger.LogWarning("Approve debit failed ticket {TicketId}: {Error}", ticketId, err);
            return ServiceResult<WithdrawalTicketResponse>.BadRequest(
                string.IsNullOrEmpty(err) ? "Không thể trừ ví" : err);
        }

        ticket.Status = WithdrawalTicketStatus.Approved;
        ticket.ReviewedByAccountId = request.AdminAccountId;
        ticket.AdminNote = request.Note;
        ticket.ReviewedAt = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        return ServiceResult<WithdrawalTicketResponse>.Success(Map(ticket), "Đã duyệt và trừ ví");
    }

    public async Task<ServiceResult<WithdrawalTicketResponse>> RejectAsync(Guid ticketId, AdminReviewWithdrawalRequest request)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ServiceResult<WithdrawalTicketResponse>.NotFound("Ticket không tồn tại");
        if (ticket.Status != WithdrawalTicketStatus.Pending)
            return ServiceResult<WithdrawalTicketResponse>.BadRequest("Trạng thái ticket không hợp lệ");

        ticket.Status = WithdrawalTicketStatus.Rejected;
        ticket.ReviewedByAccountId = request.AdminAccountId;
        ticket.AdminNote = request.Note;
        ticket.ReviewedAt = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        return ServiceResult<WithdrawalTicketResponse>.Success(Map(ticket));
    }

    /// <summary>Ghi nhận đã CK tay ra ngân hàng. Tiền đã trừ ví khi duyệt (Approve).</summary>
    public async Task<ServiceResult<WithdrawalTicketResponse>> MarkPaidAsync(Guid ticketId, AdminReviewWithdrawalRequest request)
    {
        var ticket = await _tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            return ServiceResult<WithdrawalTicketResponse>.NotFound("Ticket không tồn tại");
        if (ticket.Status != WithdrawalTicketStatus.Approved)
            return ServiceResult<WithdrawalTicketResponse>.BadRequest("Ticket phải ở trạng thái Approved");

        ticket.Status = WithdrawalTicketStatus.Paid;
        ticket.ReviewedByAccountId = request.AdminAccountId;
        if (!string.IsNullOrWhiteSpace(request.Note))
            ticket.AdminNote = request.Note;
        ticket.PaidAt = DateTime.UtcNow;
        await _tickets.UpdateAsync(ticket);
        return ServiceResult<WithdrawalTicketResponse>.Success(Map(ticket), "Đã xác nhận chuyển khoản");
    }

    private static WithdrawalTicketResponse Map(WithdrawalTicket t) => new()
    {
        TicketId = t.TicketId,
        SellerAccountId = t.SellerAccountId,
        ShopId = t.ShopId,
        AmountVnd = t.AmountVnd,
        BankName = t.BankName,
        BankAccountNumber = t.BankAccountNumber,
        BankAccountHolder = t.BankAccountHolder,
        Status = t.Status.ToString(),
        ReviewedByAccountId = t.ReviewedByAccountId,
        AdminNote = t.AdminNote,
        CreatedAt = t.CreatedAt,
        ReviewedAt = t.ReviewedAt,
        PaidAt = t.PaidAt
    };
}
