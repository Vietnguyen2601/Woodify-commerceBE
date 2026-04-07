using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.Repositories;

/// <summary>
/// Implementation của Payment Repository
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid paymentId)
    {
        return await _context.Payments.FindAsync(paymentId);
    }

    public async Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == providerPaymentId);
    }

    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Payments
            .Where(p => p.AccountId == accountId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        payment.PaymentId = Guid.NewGuid();
        payment.CreatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        payment.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<IEnumerable<Payment>> GetAllProcessingAsync()
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Processing)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
