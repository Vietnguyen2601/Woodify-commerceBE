using IdentityService.Domain.Entities;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace IdentityService.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateJSONWebToken(Account user);
    }
}