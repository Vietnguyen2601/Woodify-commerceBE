namespace PaymentService.Application.Interfaces;

/// <summary>
/// Trigger để kích hoạt polling session khi có payment mới được tạo
/// </summary>
public interface IPaymentPollingTrigger
{
    void Trigger();
}
