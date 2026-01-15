using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using IdentityService.Application.Interfaces;

namespace IdentityService.Application.Services;

public class JWTTokenService : IJwtTokenService
{
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _expireMinutes;

    public JWTTokenService(IConfiguration config)
    {
        _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
        _expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES")!);
    }

    public string GenerateJSONWebToken(Account account)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role?.RoleName ?? "Customer")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expireMinutes),
            SigningCredentials = creds,
            Issuer = _jwtIssuer,
            Audience = _jwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
