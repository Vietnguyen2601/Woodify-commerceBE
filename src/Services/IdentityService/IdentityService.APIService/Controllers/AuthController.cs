using IdentityService.APIService.Extensions;
using IdentityService.Application.Constants;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;
using System.Security.Claims;

namespace IdentityService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenService _authenService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IAuthenService authenService, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _authenService = authenService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    #region Current User
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ServiceResult<CurrentUserResponse>>> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ServiceResult<CurrentUserResponse>.Unauthorized(AuthMessages.InvalidUserToken));
        }

        var (success, account, errorMessage, errorCode) = await _authenService.GetCurrentUserAsync(userId);

        if (!success)
        {
            // Use structured error codes instead of string parsing
            return errorCode switch
            {
                IdentityService.Application.Constants.ErrorCode.AccountNotFound =>
                    NotFound(ServiceResult<CurrentUserResponse>.NotFound(errorMessage)),

                IdentityService.Application.Constants.ErrorCode.AccountNotActive =>
                    Unauthorized(ServiceResult<CurrentUserResponse>.Unauthorized(errorMessage)),

                _ => BadRequest(ServiceResult<CurrentUserResponse>.BadRequest(errorMessage ?? "Unknown error"))
            };
        }

        var response = new CurrentUserResponse(
            true,
            AuthMessages.LoginSuccess,
            account!.AccountId,
            account.Email,
            account.Username,
            account.Name,
            account.Gender,
            account.Dob,
            account.Address,
            account.PhoneNumber,
            account.Avatar
        );

        return Ok(ServiceResult<CurrentUserResponse>.Success(response, AuthMessages.LoginSuccess));
    }
    #endregion

    #region OTP Endpoints
    /// <summary>
    /// Gửi OTP để xác minh email khi đăng ký tài khoản mới
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<ActionResult<ServiceResult<OtpSentResponse>>> SendOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<OtpSentResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var result = await _authenService.SendOtpAsync(request.Email);

        if (!result)
        {
            return BadRequest(ServiceResult<OtpSentResponse>.BadRequest(AuthMessages.EmailAlreadyRegistered));
        }

        return Ok(ServiceResult<OtpSentResponse>.Success(
            new OtpSentResponse(true, AuthMessages.OtpSentSuccess),
            AuthMessages.OtpSentSuccess
        ));
    }

    /// <summary>
    /// Xác minh OTP khi đăng ký tài khoản mới
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<ActionResult<ServiceResult<OtpVerifyResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<OtpVerifyResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var result = await _authenService.VerifyOtpAsync(request.Email, request.Otp);

        if (!result)
        {
            return BadRequest(ServiceResult<OtpVerifyResponse>.BadRequest(AuthMessages.OtpInvalidOrExpired));
        }

        await _authenService.MarkOtpVerified(request.Email);

        return Ok(ServiceResult<OtpVerifyResponse>.Success(
            new OtpVerifyResponse(true, AuthMessages.OtpVerifySuccess),
            AuthMessages.OtpVerifySuccess
        ));
    }

    /// <summary>
    /// Kiểm tra email đã được xác minh OTP chưa
    /// </summary>
    [HttpGet("check-email-verified/{email}")]
    public async Task<ActionResult<ServiceResult<bool>>> CheckEmailVerified(string email)
    {
        var isVerified = await _authenService.IsEmailVerifiedAsync(email);

        return Ok(ServiceResult<bool>.Success(
            isVerified,
            isVerified ? AuthMessages.EmailVerified : AuthMessages.EmailNotVerified
        ));
    }
    #endregion

    #region Register & Login Endpoints
    /// <summary>
    /// Đăng ký tài khoản mới (yêu cầu đã xác minh OTP trước đó)
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ServiceResult<RegisterResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<RegisterResponse>.BadRequest(AuthMessages.InvalidData));
        }

        // Kiểm tra mật khẩu xác nhận
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(ServiceResult<RegisterResponse>.BadRequest(AuthMessages.PasswordMismatch));
        }

        var (success, accountId, errorMessage, errorCode) = await _authenService.RegisterAsync(request.Email, request.Password, request.Username, request.Address);

        if (!success)
        {
            // Use structured error codes for appropriate HTTP status codes
            var statusCode = errorCode switch
            {
                IdentityService.Application.Constants.ErrorCode.EmailAlreadyRegistered => 409,   // Conflict
                IdentityService.Application.Constants.ErrorCode.UsernameAlreadyExists => 409,    // Conflict
                IdentityService.Application.Constants.ErrorCode.OtpNotVerified => 400,           // Bad Request
                _ => 400  // Bad Request
            };

            var result = statusCode == 409
                ? ServiceResult<RegisterResponse>.Conflict(errorMessage ?? AuthMessages.InvalidData)
                : ServiceResult<RegisterResponse>.BadRequest(errorMessage ?? AuthMessages.InvalidData);

            return StatusCode(statusCode, result);
        }

        return Ok(ServiceResult<RegisterResponse>.Success(
            new RegisterResponse(true, AuthMessages.RegisterSuccess, accountId),
            AuthMessages.RegisterSuccess
        ));
    }

    /// <summary>
    /// Đăng nhập với email và mật khẩu
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ServiceResult<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<LoginResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var (success, account, errorMessage, errorCode) = await _authenService.LoginAsync(request.Email, request.Password);

        if (!success || account == null)
        {
            // Use structured error codes for better error handling
            var statusCode = errorCode switch
            {
                IdentityService.Application.Constants.ErrorCode.AccountNotActive => 401, // Unauthorized
                IdentityService.Application.Constants.ErrorCode.InvalidCredentials => 401, // Unauthorized
                _ => 400 // Bad Request
            };

            var result = statusCode == 401
                ? ServiceResult<LoginResponse>.Unauthorized(errorMessage ?? AuthMessages.InvalidCredentials)
                : ServiceResult<LoginResponse>.BadRequest(errorMessage ?? AuthMessages.InvalidCredentials);

            return StatusCode(statusCode, result);
        }

        // Generate JWT tokens
        var token = _jwtTokenService.GenerateJSONWebToken(account);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(account);

        // Set JWT tokens as HttpOnly Cookies (for additional security)
        Response.SetJwtCookies(token, refreshToken);

        // Return response WITH tokens (both in cookies and response body)
        return Ok(ServiceResult<LoginResponse>.Success(
            new LoginResponse(true, AuthMessages.LoginSuccess, account.AccountId, account.Email, account.Username, token, refreshToken),
            AuthMessages.LoginSuccess
        ));
    }
    /// <summary>
    /// Làm mới access token bằng refresh token (cookie-based)
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ServiceResult<RefreshTokenResponse>>> RefreshToken()
    {
        // Get refresh token from HttpOnly cookie
        if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ServiceResult<RefreshTokenResponse>.Unauthorized(AuthMessages.RefreshTokenInvalid));
        }

        // Validate refresh token
        var principal = _jwtTokenService.ValidateRefreshToken(refreshToken);
        if (principal == null)
        {
            return Unauthorized(ServiceResult<RefreshTokenResponse>.Unauthorized(AuthMessages.RefreshTokenInvalid));
        }

        // Get AccountId from refresh token claims
        var accountIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
        {
            return Unauthorized(ServiceResult<RefreshTokenResponse>.Unauthorized(AuthMessages.RefreshTokenInvalid));
        }

        // Lookup account
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
        if (account == null || !account.IsActive)
        {
            return Unauthorized(ServiceResult<RefreshTokenResponse>.Unauthorized(AuthMessages.RefreshTokenInvalid));
        }

        // Load Role for JWT claims
        if (account.RoleId.HasValue)
        {
            account.Role = await _unitOfWork.Roles.GetByIdAsync(account.RoleId.Value);
        }

        // Generate new token pair
        var newAccessToken = _jwtTokenService.GenerateJSONWebToken(account);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(account);

        // Set new JWT tokens as HttpOnly Cookies
        Response.SetJwtCookies(newAccessToken, newRefreshToken);

        // Return response without tokens (they are now in secure cookies)
        return Ok(ServiceResult<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(true, AuthMessages.RefreshTokenSuccess, null, null),
            AuthMessages.RefreshTokenSuccess
        ));
    }
    #endregion

    #region Forgot Password Endpoints
    /// <summary>
    /// Gửi OTP để đặt lại mật khẩu (dành cho người dùng đã có tài khoản)
    /// </summary>
    [HttpPost("forgot-password/send-otp")]
    public async Task<ActionResult<ServiceResult<OtpSentResponse>>> SendResetPasswordOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<OtpSentResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var result = await _authenService.SendResetPasswordOtpAsync(request.Email);

        if (!result)
        {
            return NotFound(ServiceResult<OtpSentResponse>.NotFound(AuthMessages.EmailNotFound));
        }

        return Ok(ServiceResult<OtpSentResponse>.Success(
            new OtpSentResponse(true, AuthMessages.ResetOtpSentSuccess),
            AuthMessages.ResetOtpSentSuccess
        ));
    }

    /// <summary>
    /// Xác minh OTP đặt lại mật khẩu và nhận reset token
    /// </summary>
    [HttpPost("forgot-password/verify-otp")]
    public async Task<ActionResult<ServiceResult<OtpVerifyResponse>>> VerifyResetPasswordOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<OtpVerifyResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var (success, resetToken, errorMessage, errorCode) = await _authenService.VerifyResetPasswordOtpWithTokenAsync(request.Email, request.Otp);

        if (!success)
        {
            return BadRequest(ServiceResult<OtpVerifyResponse>.BadRequest(errorMessage ?? AuthMessages.OtpInvalidOrExpired));
        }

        return Ok(ServiceResult<OtpVerifyResponse>.Success(
            new OtpVerifyResponse(true, AuthMessages.ResetOtpVerifySuccess, resetToken),
            AuthMessages.ResetOtpVerifySuccess
        ));
    }

    /// <summary>
    /// Đặt lại mật khẩu sử dụng reset token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ServiceResult<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<ResetPasswordResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var result = await _authenService.ResetPasswordWithTokenAsync(request.ResetToken, request.NewPassword);

        if (!result)
        {
            return BadRequest(ServiceResult<ResetPasswordResponse>.BadRequest(AuthMessages.ResetTokenInvalidOrExpired));
        }

        return Ok(ServiceResult<ResetPasswordResponse>.Success(
            new ResetPasswordResponse(true, AuthMessages.ResetPasswordSuccess),
            AuthMessages.ResetPasswordSuccess
        ));
    }

    /// <summary>
    /// Đăng xuất - Xóa JWT Cookies
    /// </summary>
    [HttpPost("logout")]
    public ActionResult<ServiceResult<object>> Logout()
    {
        // Clear JWT cookies
        Response.ClearJwtCookies();

        return Ok(ServiceResult<object>.Success(
            null,
            "Logout successful"
        ));
    }
    #endregion
}
