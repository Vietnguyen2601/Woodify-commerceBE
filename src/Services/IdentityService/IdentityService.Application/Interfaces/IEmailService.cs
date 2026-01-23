using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }
}