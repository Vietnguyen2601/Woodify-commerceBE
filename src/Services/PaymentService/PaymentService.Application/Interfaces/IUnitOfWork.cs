namespace PaymentService.Application.Interfaces;

/// <summary>
/// Unit of Work pattern - Quản lý database transaction
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Lưu tất cả thay đổi vào database
    /// </summary>
    Task<int> SaveChangesAsync();
}
