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

public record RegisterRequest(
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    string Password,

    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    string ConfirmPassword,

    [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
    string Username
);

public record LoginRequest(
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    string Password
);

public record ResetPasswordRequest(
    [Required(ErrorMessage = "Reset token là bắt buộc")]
    string ResetToken,

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

public record RegisterResponse(
    bool Success,
    string Message,
    Guid? AccountId = null
);

public record LoginResponse(
    bool Success,
    string Message,
    Guid? AccountId = null,
    string? Email = null,
    string? Username = null,
    string? Token = null
);

public record ResetPasswordResponse(
    bool Success,
    string Message
);
