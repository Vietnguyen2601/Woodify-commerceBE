using IdentityService.Application.Constants;
using IdentityService.Application.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Interfaces;

public interface IAuthenService
{
    // OTP
    Task<bool> SendOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task<bool> IsOtpVerified(string email);
    Task<bool> IsEmailVerifiedAsync(string email);
    Task MarkOtpVerified(string email);

    // Register & Login
    Task<(bool Success, Guid? AccountId, string? ErrorMessage, ErrorCode ErrorCode)> RegisterAsync(string email, string password, string username, string? address = null);
    Task<(bool Success, Account? Account, string? ErrorMessage, ErrorCode ErrorCode)> LoginAsync(string email, string password);

    // Current User
    Task<(bool Success, Account? Account, string? ErrorMessage, ErrorCode ErrorCode)> GetCurrentUserAsync(Guid userId);

    // Forgot Password
    Task<bool> SendResetPasswordOtpAsync(string email);
    Task<bool> VerifyResetPasswordOtpAsync(string email, string otp);
    Task<(bool Success, string? ResetToken, string? ErrorMessage, ErrorCode ErrorCode)> VerifyResetPasswordOtpWithTokenAsync(string email, string otp);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
    Task<bool> ResetPasswordWithTokenAsync(string resetToken, string newPassword);
}
