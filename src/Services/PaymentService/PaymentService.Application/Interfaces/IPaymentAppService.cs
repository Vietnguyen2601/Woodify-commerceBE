using PaymentService.Application.DTOs;
using Shared.Results;

namespace PaymentService.Application.Interfaces;

/// <summary>
/// Interface cho Payment Application Service
/// Xử lý business logic thanh toán
/// </summary>
public interface IPaymentAppService
{
    /// <summary>
    /// Tạo link thanh toán PayOS
    /// </summary>
    Task<ServiceResult<CreatePaymentLinkResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest request);

    /// <summary>
    /// Query thông tin thanh toán theo orderCode
    /// </summary>
    Task<ServiceResult<PaymentInfoResponse>> GetPaymentByOrderCodeAsync(long orderCode);

    /// <summary>
    /// Query thông tin thanh toán theo PaymentId
    /// </summary>
    Task<ServiceResult<PaymentInfoResponse>> GetPaymentByIdAsync(Guid paymentId);
}
