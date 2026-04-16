using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;

namespace PaymentService.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;

    public UnitOfWork(PaymentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lưu tất cả thay đổi vào database
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Dispose DbContext
    /// </summary>
    public void Dispose()
    {
        _context?.Dispose();
    }
}
