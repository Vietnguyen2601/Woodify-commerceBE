using Microsoft.AspNetCore.Http;
using System;

namespace IdentityService.APIService.Extensions
{
    public static class JwtCookieExtensions
    {
        /// <summary>
        /// Set JWT Access Token and Refresh Token as HttpOnly Cookies
        /// </summary>
        public static void SetJwtCookies(this HttpResponse response, string accessToken, string refreshToken)
        {
            SetAccessTokenCookie(response, accessToken);
            SetRefreshTokenCookie(response, refreshToken);
        }

        /// <summary>
        /// Set JWT Access Token as HttpOnly Cookie
        /// </summary>
        public static void SetAccessTokenCookie(this HttpResponse response, string accessToken)
        {
            var accessTokenExpires = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES") ?? "15");
            
            response.Cookies.Append(
                "AccessToken",
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,                              // ✅ Chống XSS - JS không thể access
                    Secure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production",  // ✅ Chỉ qua HTTPS (production)
                    SameSite = SameSiteMode.Strict,              // ✅ Chống CSRF
                    Expires = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpires),
                    Path = "/"
                }
            );
        }

        /// <summary>
        /// Set JWT Refresh Token as HttpOnly Cookie
        /// </summary>
        public static void SetRefreshTokenCookie(this HttpResponse response, string refreshToken)
        {
            var refreshTokenExpireDays = int.Parse(Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRE_DAYS") ?? "7");
            
            response.Cookies.Append(
                "RefreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,                              // ✅ Chống XSS
                    Secure = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production",  // ✅ HTTPS only
                    SameSite = SameSiteMode.Strict,              // ✅ Chống CSRF
                    Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenExpireDays),
                    Path = "/"
                }
            );
        }

        /// <summary>
        /// Clear JWT Cookies (for logout)
        /// </summary>
        public static void ClearJwtCookies(this HttpResponse response)
        {
            response.Cookies.Delete("AccessToken");
            response.Cookies.Delete("RefreshToken");
        }
    }
}
