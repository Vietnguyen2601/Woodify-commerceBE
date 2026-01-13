using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using IdentityService.Application.Interfaces;


namespace IdentityService.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _useSsl;
        public EmailService(IConfiguration configuration)
        {
            _fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM")
                ?? configuration["EmailSettings:FromEmail"]
                ?? throw new InvalidOperationException("EMAIL_FROM is not configured");
            _fromPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                ?? configuration["EmailSettings:FromPassword"]
                ?? throw new InvalidOperationException("EMAIL_PASSWORD is not configured");
            _smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST")
                ?? configuration["EmailSettings:SmtpHost"]
                ?? "smtp.gmail.com";
            _smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT")
                ?? configuration["EmailSettings:SmtpPort"]
                ?? "587");
            _useSsl = bool.Parse(Environment.GetEnvironmentVariable("EMAIL_USE_SSL")
                ?? configuration["EmailSettings:UseSsl"]
                ?? "true");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                    client.EnableSsl = _useSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email send failed: {ex.Message}");
                return false;
            }
        }
    }
}