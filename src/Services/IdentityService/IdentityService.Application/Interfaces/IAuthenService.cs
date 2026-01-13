namespace IdentityService.Application.Interfaces;

public interface IAuthenService
{
    Task<bool> SendOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
    Task<bool> SendResetPasswordOtpAsync(string email);
    Task<bool> VerifyResetPasswordOtpAsync(string email, string otp);
    Task<bool> ResetPasswordAsync(string email, string newPassword);
    Task<bool> IsOtpVerified(string email);
    Task<bool> IsEmailVerifiedAsync(string email);
    Task MarkOtpVerified(string email);
    Task<(bool Success, string? ResetToken)> VerifyResetPasswordOtpWithTokenAsync(string email, string otp);
    Task<bool> ResetPasswordWithTokenAsync(string resetToken, string newPassword);
}
