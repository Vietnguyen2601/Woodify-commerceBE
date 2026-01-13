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

    public AuthController(IAuthenService authenService)
    {
        _authenService = authenService;
    }

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
    /// Đặt lại mật khẩu sử dụng reset token (khuyên dùng - an toàn hơn)
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
    /// Đặt lại mật khẩu sử dụng email trực tiếp (legacy - yêu cầu OTP đã xác minh trước đó)
    /// </summary>
    [HttpPost("reset-password-legacy")]
    public async Task<ActionResult<ServiceResult<ResetPasswordResponse>>> ResetPasswordLegacy([FromBody] ResetPasswordWithEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ServiceResult<ResetPasswordResponse>.BadRequest(AuthMessages.InvalidData));
        }

        var isVerified = await _authenService.IsEmailVerifiedAsync(request.Email);
        if (!isVerified)
        {
            return BadRequest(ServiceResult<ResetPasswordResponse>.BadRequest(AuthMessages.OtpNotVerified));
        }

        var result = await _authenService.ResetPasswordAsync(request.Email, request.NewPassword);

        if (!result)
        {
            return NotFound(ServiceResult<ResetPasswordResponse>.NotFound(AuthMessages.AccountNotFound));
        }

        return Ok(ServiceResult<ResetPasswordResponse>.Success(
            new ResetPasswordResponse(true, AuthMessages.ResetPasswordSuccess),
            AuthMessages.ResetPasswordSuccess
        ));
    }
}
