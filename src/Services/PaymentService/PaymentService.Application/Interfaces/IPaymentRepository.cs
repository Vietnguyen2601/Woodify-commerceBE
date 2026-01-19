using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

/// <summary>
/// Interface cho Payment Repository - định nghĩa ở Application layer
/// Implementation sẽ ở Infrastructure layer
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Lấy Payment theo ID
    /// </summary>
    Task<Payment?> GetByIdAsync(Guid paymentId);

    /// <summary>
    /// Lấy Payment theo provider payment ID (orderCode của PayOS)
    /// </summary>
    Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId);

    /// <summary>
    /// Lấy tất cả Payment của một Order
    /// </summary>
    Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Lấy tất cả Payment của một Account
    /// </summary>
    Task<IEnumerable<Payment>> GetByAccountIdAsync(Guid accountId);

    /// <summary>
    /// Tạo mới Payment
    /// </summary>
    Task<Payment> CreateAsync(Payment payment);

    /// <summary>
    /// Cập nhật Payment
    /// </summary>
    Task<Payment> UpdateAsync(Payment payment);
}
