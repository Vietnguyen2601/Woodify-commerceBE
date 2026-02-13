using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailBackgroundQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailBackgroundQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var emailMessage = await _queue.DequeueEmailAsync(stoppingToken);

                    // Create a new scope for each email since EmailService is Scoped
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var success = await emailService.SendEmailAsync(
                        emailMessage.To,
                        emailMessage.Subject,
                        emailMessage.Body);

                    if (success)
                    {
                        _logger.LogInformation("Email sent successfully to {To}", emailMessage.To);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send email to {To}", emailMessage.To);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending email in background.");
                    // Wait before retrying to avoid tight loop on persistent errors
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}
