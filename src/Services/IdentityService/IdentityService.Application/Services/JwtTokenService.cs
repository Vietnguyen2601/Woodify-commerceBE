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
    private readonly string _jwtRefreshKey;
    private readonly int _refreshExpireDays;

    public JWTTokenService(IConfiguration config)
    {
        _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")!;
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
        _expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES")!);
        _jwtRefreshKey = Environment.GetEnvironmentVariable("JWT_REFRESH_KEY") ?? _jwtKey + "-refresh";
        _refreshExpireDays = int.Parse(Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRE_DAYS") ?? "7");
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

    public string GenerateRefreshToken(Account account)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim("token_type", "refresh")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtRefreshKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_refreshExpireDays),
            SigningCredentials = creds,
            Issuer = _jwtIssuer,
            Audience = _jwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtRefreshKey));

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtIssuer,
                ValidAudience = _jwtAudience,
                IssuerSigningKey = key
            }, out var validatedToken);

            // Verify this is actually a refresh token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "refresh")
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
