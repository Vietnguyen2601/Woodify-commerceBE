using System.Threading;
using System.Threading.Tasks;

namespace IdentityService.Application.Interfaces
{
    public record EmailMessage(string To, string Subject, string Body);

    public interface IEmailBackgroundQueue
    {
        ValueTask QueueEmailAsync(EmailMessage email);
        ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken);
    }
}
