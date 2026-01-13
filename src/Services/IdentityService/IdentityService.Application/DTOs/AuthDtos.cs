using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.DTOs;

// Request DTOs
public record SendOtpRequest(
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email
);

public record VerifyOtpRequest(
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "OTP là bắt buộc")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải có 6 ký tự")]
    string Otp
);

public record ResetPasswordRequest(
    [Required(ErrorMessage = "Reset token là bắt buộc")]
    string ResetToken,

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    string NewPassword
);

public record ResetPasswordWithEmailRequest(
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    string NewPassword
);

// Response DTOs
public record OtpSentResponse(
    bool Success,
    string Message
);

public record OtpVerifyResponse(
    bool Success,
    string Message,
    string? ResetToken = null
);

public record ResetPasswordResponse(
    bool Success,
    string Message
);
