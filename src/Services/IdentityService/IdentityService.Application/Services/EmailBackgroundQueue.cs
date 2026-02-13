using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Services
{
    public class EmailBackgroundQueue : IEmailBackgroundQueue
    {
        private readonly Channel<EmailMessage> _queue;

        public EmailBackgroundQueue()
        {
            _queue = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
        }

        public async ValueTask QueueEmailAsync(EmailMessage email)
        {
            await _queue.Writer.WriteAsync(email);
        }

        public async ValueTask<EmailMessage> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
