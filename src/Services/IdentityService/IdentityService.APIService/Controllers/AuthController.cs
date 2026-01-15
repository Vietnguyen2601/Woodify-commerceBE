using IdentityService.Application.Constants;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

namespace IdentityService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenService _authenService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthenService authenService, IJwtTokenService jwtTokenService)
    {
        _authenService = authenService;
        _jwtTokenService = jwtTokenService;
    }

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

        var (success, accountId, errorMessage) = await _authenService.RegisterAsync(request.Email, request.Password, request.Username);

        if (!success)
        {
            return BadRequest(ServiceResult<RegisterResponse>.BadRequest(errorMessage ?? AuthMessages.InvalidData));
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

        var (success, account, errorMessage) = await _authenService.LoginAsync(request.Email, request.Password);

        if (!success || account == null)
        {
            return Unauthorized(ServiceResult<LoginResponse>.Unauthorized(errorMessage ?? AuthMessages.InvalidCredentials));
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateJSONWebToken(account);

        return Ok(ServiceResult<LoginResponse>.Success(
            new LoginResponse(true, AuthMessages.LoginSuccess, account.AccountId, account.Email, account.Username, token),
            AuthMessages.LoginSuccess
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

        var (success, resetToken) = await _authenService.VerifyResetPasswordOtpWithTokenAsync(request.Email, request.Otp);

        if (!success)
        {
            return BadRequest(ServiceResult<OtpVerifyResponse>.BadRequest(AuthMessages.OtpInvalidOrExpired));
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
    #endregion
}
