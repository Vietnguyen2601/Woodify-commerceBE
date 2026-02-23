using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IdentityService.APIService.Middlewares
{
    /// <summary>
    /// Middleware to extract JWT token from HttpOnly cookie and add it to Authorization header
    /// This allows [Authorize] attribute to work with cookie-based tokens
    /// </summary>
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if Authorization header is not set and try to get access token from HttpOnly cookie
            if (string.IsNullOrEmpty(context.Request.Headers["Authorization"])
                && context.Request.Cookies.TryGetValue("AccessToken", out var accessToken))
            {
                // Add token to Authorization header as Bearer token
                context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register JWT Cookie Middleware
    /// </summary>
    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}
