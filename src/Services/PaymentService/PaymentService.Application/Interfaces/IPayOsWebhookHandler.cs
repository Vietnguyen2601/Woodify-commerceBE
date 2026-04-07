using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

/// <summary>
/// Service xử lý PayOS webhook callback
/// </summary>
public interface IPayOsWebhookHandler
{
    /// <summary>
    /// Xử lý webhook callback từ PayOS
    /// </summary>
    Task<PayOsWebhookResponse> HandleWebhookAsync(PayOsWebhookData webhook);
}
