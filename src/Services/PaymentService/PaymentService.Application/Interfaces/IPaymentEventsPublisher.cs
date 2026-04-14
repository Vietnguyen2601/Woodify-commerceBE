using Shared.Events;

namespace PaymentService.Application.Interfaces;

public interface IPaymentEventsPublisher
{
    void PublishPaymentOrdersPaid(PaymentOrdersPaidEvent evt);
}
