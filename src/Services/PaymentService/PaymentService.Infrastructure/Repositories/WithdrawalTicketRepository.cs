using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

public class WithdrawalTicketRepository : IWithdrawalTicketRepository
{
    private readonly PaymentDbContext _context;

    public WithdrawalTicketRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<WithdrawalTicket> AddAsync(WithdrawalTicket ticket, CancellationToken cancellationToken = default)
    {
        ticket.TicketId = Guid.NewGuid();
        ticket.CreatedAt = DateTime.UtcNow;
        _context.WithdrawalTickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task<WithdrawalTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _context.WithdrawalTickets.FirstOrDefaultAsync(t => t.TicketId == ticketId, cancellationToken);
    }

    public async Task<(IReadOnlyList<WithdrawalTicket> Items, int Total)> ListAsync(
        WithdrawalTicketStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var q = _context.WithdrawalTickets.AsQueryable();
        if (status.HasValue)
            q = q.Where(t => t.Status == status.Value);

        var total = await q.CountAsync(cancellationToken);
        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<WithdrawalTicket> Items, int TotalInDb)> GetAllAsync(
        WithdrawalTicketStatus? status,
        int maxRows = 10_000,
        CancellationToken cancellationToken = default)
    {
        var q = _context.WithdrawalTickets.AsQueryable();
        if (status.HasValue)
            q = q.Where(t => t.Status == status.Value);

        var totalInDb = await q.CountAsync(cancellationToken);
        var take = Math.Clamp(maxRows, 1, 50_000);
        var items = await q
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalInDb);
    }

    public async Task UpdateAsync(WithdrawalTicket ticket, CancellationToken cancellationToken = default)
    {
        _context.WithdrawalTickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
