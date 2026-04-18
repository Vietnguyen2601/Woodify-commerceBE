using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Interfaces;

public interface IWithdrawalTicketRepository
{
    Task<WithdrawalTicket> AddAsync(WithdrawalTicket ticket, CancellationToken cancellationToken = default);
    Task<WithdrawalTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<WithdrawalTicket> Items, int Total)> ListAsync(
        WithdrawalTicketStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tất cả ticket (mới nhất trước), tối đa <paramref name="maxRows"/> bản ghi. <paramref name="totalInDb"/> = tổng khớp filter.
    /// </summary>
    Task<(IReadOnlyList<WithdrawalTicket> Items, int TotalInDb)> GetAllAsync(
        WithdrawalTicketStatus? status,
        int maxRows = 10_000,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(WithdrawalTicket ticket, CancellationToken cancellationToken = default);
}
