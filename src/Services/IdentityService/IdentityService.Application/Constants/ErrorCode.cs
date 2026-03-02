namespace IdentityService.Application.Constants;

/// <summary>
/// Structured error codes for authentication and account operations.
/// Replaces error message string parsing to enable robust error handling across client/server boundaries.
/// </summary>
public enum ErrorCode
{
    /// <summary>No error</summary>
    None = 0,

    /// <summary>Account not found by ID or email</summary>
    AccountNotFound = 1001,

    /// <summary>Account is not active</summary>
    AccountNotActive = 1002,

    /// <summary>Email already registered</summary>
    EmailAlreadyRegistered = 1003,

    /// <summary>Invalid credentials (email/password)</summary>
    InvalidCredentials = 1004,

    /// <summary>OTP not verified</summary>
    OtpNotVerified = 1005,

    /// <summary>OTP invalid or expired</summary>
    OtpInvalidOrExpired = 1006,

    /// <summary>Username already exists</summary>
    UsernameAlreadyExists = 1007,

    /// <summary>Email not found for password reset</summary>
    EmailNotFound = 1008,

    /// <summary>Reset token invalid or expired</summary>
    ResetTokenInvalidOrExpired = 1009,

    /// <summary>Generic validation error</summary>
    ValidationError = 4000,

    /// <summary>Internal server error</summary>
    InternalServerError = 5000
}
