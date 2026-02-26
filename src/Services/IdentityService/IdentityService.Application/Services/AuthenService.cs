using System.Collections.Concurrent;
using IdentityService.Application.Constants;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Shared.Events;
using Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailBackgroundQueue _emailBackgroundQueue;
        private readonly IPasswordHasher _passwordHasher;
        private readonly RabbitMQPublisher? _publisher;
        private readonly ILogger<AuthenService>? _logger;
        private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _otpStorage = new();
        private static readonly ConcurrentDictionary<string, bool> _verifiedEmails = new();
        private static readonly ConcurrentDictionary<string, (string Email, DateTime Expiry)> _resetTokens = new();

        public AuthenService(IUnitOfWork unitOfWork, IEmailBackgroundQueue emailBackgroundQueue, IPasswordHasher passwordHasher, RabbitMQPublisher? publisher = null, ILogger<AuthenService>? logger = null)
        {
            _unitOfWork = unitOfWork;
            _emailBackgroundQueue = emailBackgroundQueue;
            _passwordHasher = passwordHasher;
            _publisher = publisher;
            _logger = logger;
        }

        #region OTP Methods
        public async Task<bool> SendOtpAsync(string email)
        {
            var user = await _unitOfWork.Accounts.GetByEmailAsync(email);
            if (user != null) return false;

            string otp = new Random().Next(100000, 999999).ToString();
            _otpStorage[email] = (otp, DateTime.UtcNow.AddMinutes(5));

            string subject = "[Woodify] Xác Minh Đăng Ký Tài Khoản - OTP";
            string body = $"<html><body style='font-family:\"Segoe UI\", Tahoma, Geneva, Verdana, sans-serif; color:#2c2c2c; text-align:center; margin:0; padding:0; background:linear-gradient(135deg, #f5ede3 0%, #ede0d5 100%);'>"
                        + "<div style='max-width:700px; margin:40px auto; padding:0; border-radius:15px; background:white; box-shadow:0 10px 40px rgba(139, 90, 43, 0.15); overflow:hidden;'>"
                        + "<div style='background:linear-gradient(135deg, #8B5A2B 0%, #A0826D 100%); padding:30px 20px; text-align:center;'>"
                        + "<h1 style='color:#FFF8F0; margin:0; font-size:28px; font-weight:600;'>🌳 Woodify</h1>"
                        + "</div>"
                        + "<div style='padding:40px 30px;'>"
                        + "<h2 style='color:#8B5A2B; margin:0 0 20px 0; font-size:24px;'>Chào mừng đến với Woodify!</h2>"
                        + "<p style='color:#555; font-size:16px; line-height:1.6; margin:0 0 25px 0;'>Cảm ơn bạn đã đăng ký tài khoản tại <b>Woodify</b>. Vui lòng sử dụng mã OTP bên dưới để xác minh tài khoản của bạn:</p>"
                        + "<div style='background:linear-gradient(135deg, #FFF8F0 0%, #F5EDE3 100%); padding:20px; border-radius:10px; margin:30px 0;'>"
                        + "<p style='color:#8B5A2B; font-size:14px; margin:0 0 15px 0;'>Mã OTP của bạn:</p>"
                        + "<h3 style='color:#FFF; font-size:42px; background:linear-gradient(135deg, #8B5A2B 0%, #A0826D 100%); display:inline-block; padding:15px 35px; border-radius:8px; margin:0; letter-spacing:5px; font-family:monospace; font-weight:bold;'>" + otp + "</h3>"
                        + "</div>"
                        + "<p style='color:#666; font-size:14px; line-height:1.5; margin:25px 0;'><i>⏱️ Mã OTP này có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này với bất kỳ ai.</i></p>"
                        + "<hr style='border:none; border-top:2px solid #E8D7C3; margin:30px 0;'>"
                        + "<p style='color:#777; font-size:13px; line-height:1.6; margin:20px 0;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>"
                        + "<p style='color:#8B5A2B; margin-top:30px; font-weight:600;'>Trân trọng,<br><b>Đội ngũ Woodify</b></p>"
                        + "</div>"
                        + "<div style='background:#f5ede3; padding:20px; text-align:center; border-top:1px solid #E8D7C3;'>"
                        + "<p style='color:#999; font-size:12px; margin:0;'>© 2026 Woodify. Tất cả quyền được bảo lưu.</p>"
                        + "</div>"
                        + "</div></body></html>";

            await _emailBackgroundQueue.QueueEmailAsync(new EmailMessage(email, subject, body));
            return true;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            if (_otpStorage.TryGetValue(email, out var storedOtp) && storedOtp.Otp == otp && storedOtp.Expiry > DateTime.UtcNow)
            {
                _otpStorage.Remove(email, out _);
                return true;
            }
            return false;
        }

        public async Task<bool> IsOtpVerified(string email)
        {
            return _verifiedEmails.TryGetValue(email, out var isVerified) && isVerified;
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            return _verifiedEmails.TryGetValue(email, out var isVerified) && isVerified;
        }

        public async Task MarkOtpVerified(string email)
        {
            _verifiedEmails[email] = true;
        }
        #endregion

        #region Register & Login
        public async Task<(bool Success, Guid? AccountId, string? ErrorMessage)> RegisterAsync(string email, string password, string username, string? address = null)
        {
            // Kiểm tra email đã xác minh OTP chưa
            if (!_verifiedEmails.TryGetValue(email, out var isVerified) || !isVerified)
            {
                return (false, null, AuthMessages.OtpNotVerifiedForRegister);
            }

            // Kiểm tra email đã tồn tại chưa
            var existingAccount = await _unitOfWork.Accounts.GetByEmailAsync(email);
            if (existingAccount != null)
            {
                return (false, null, AuthMessages.EmailAlreadyRegistered);
            }

            // Kiểm tra username đã tồn tại chưa
            var existingUsername = await _unitOfWork.Accounts.GetByUsernameAsync(username);
            if (existingUsername != null)
            {
                return (false, null, AuthMessages.UsernameAlreadyExists);
            }

            var customerRole = await _unitOfWork.Roles.GetByNameAsync("Customer");

            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Email = email,
                Username = username,
                Password = _passwordHasher.HashPassword(password),
                Address = address ?? string.Empty,
                RoleId = customerRole?.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Accounts.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            // Xóa trạng thái verified sau khi đăng ký thành công
            _verifiedEmails.TryRemove(email, out _);

            // 🔔 Publish AccountCreatedEvent để PaymentService tạo wallet
            try
            {
                if (_publisher != null)
                {
                    var accountCreatedEvent = new AccountCreatedEvent
                    {
                        AccountId = account.AccountId,
                        Username = account.Username,
                        Email = account.Email,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Publish directly to queue, not through exchange
                    _publisher.PublishToQueue("account.created", accountCreatedEvent);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log error but don't fail the registration for expected publishing issues
                _logger?.LogError(ex, "Failed to publish AccountCreatedEvent for account {AccountId} due to an invalid operation. Registration succeeded but event was not published.", account.AccountId);
                // Event publishing failed - registration still succeeds
            }
            catch (TimeoutException ex)
            {
                // Log timeout but don't fail the registration
                _logger?.LogError(ex, "Timed out while publishing AccountCreatedEvent for account {AccountId}. Registration succeeded but event may not have been processed.", account.AccountId);
            }
            catch (IOException ex)
            {
                // Log I/O error but don't fail the registration
                _logger?.LogError(ex, "Failed to publish AccountCreatedEvent for account {AccountId} due to a network error. Registration succeeded but event was not published.", account.AccountId);
            }
            catch (Exception ex)
            {
                // Log unexpected errors and rethrow to avoid silently swallowing programming or critical errors
                _logger?.LogError(ex, "Unexpected error while publishing AccountCreatedEvent for account {AccountId}.", account.AccountId);
                throw;
            }

            return (true, account.AccountId, null);
        }

        public async Task<(bool Success, Account? Account, string? ErrorMessage)> LoginAsync(string email, string password)
        {
            var account = await _unitOfWork.Accounts.GetByEmailAsync(email);

            if (account == null)
            {
                return (false, null, AuthMessages.InvalidCredentials);
            }

            // Verify password
            if (!_passwordHasher.VerifyHashedPassword(account.Password, password))
            {
                return (false, null, AuthMessages.InvalidCredentials);
            }

            if (!account.IsActive)
            {
                return (false, null, AuthMessages.AccountNotActive);
            }

            // Load Role for JWT claims
            if (account.RoleId.HasValue)
            {
                account.Role = await _unitOfWork.Roles.GetByIdAsync(account.RoleId.Value);
            }

            return (true, account, null);
        }

        public async Task<(bool Success, Account? Account, string? ErrorMessage)> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var account = await _unitOfWork.Accounts.GetByIdAsync(userId);
                
                if (account == null)
                {
                    return (false, null, "Account not found");
                }

                if (!account.IsActive)
                {
                    return (false, null, "Account is not active");
                }

                // Load Role information
                if (account.RoleId.HasValue)
                {
                    account.Role = await _unitOfWork.Roles.GetByIdAsync(account.RoleId.Value);
                }

                return (true, account, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error retrieving current user: {ex.Message}");
            }
        }
        #endregion

        #region Forgot Password
        public async Task<bool> SendResetPasswordOtpAsync(string email)
        {
            var user = await _unitOfWork.Accounts.GetByEmailAsync(email);
            if (user == null) return false;

            string otp = new Random().Next(100000, 999999).ToString();
            _otpStorage[email] = (otp, DateTime.UtcNow.AddMinutes(5));

            string subject = "[Woodify] Xác Minh Đặt Lại Mật Khẩu - OTP";
            string body = $"<html><body style='font-family:\"Segoe UI\", Tahoma, Geneva, Verdana, sans-serif; color:#2c2c2c; text-align:center; margin:0; padding:0; background:linear-gradient(135deg, #f5ede3 0%, #ede0d5 100%);'>"
                        + "<div style='max-width:700px; margin:40px auto; padding:0; border-radius:15px; background:white; box-shadow:0 10px 40px rgba(139, 90, 43, 0.15); overflow:hidden;'>"
                        + "<div style='background:linear-gradient(135deg, #8B5A2B 0%, #A0826D 100%); padding:30px 20px; text-align:center;'>"
                        + "<h1 style='color:#FFF8F0; margin:0; font-size:28px; font-weight:600;'>🌳 Woodify</h1>"
                        + "</div>"
                        + "<div style='padding:40px 30px;'>"
                        + "<h2 style='color:#8B5A2B; margin:0 0 20px 0; font-size:24px;'>Xin chào,</h2>"
                        + "<p style='color:#555; font-size:16px; line-height:1.6; margin:0 0 25px 0;'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản <b>Woodify</b> của bạn. Vui lòng sử dụng mã OTP bên dưới để tiếp tục quá trình đặt lại mật khẩu:</p>"
                        + "<div style='background:linear-gradient(135deg, #FFF8F0 0%, #F5EDE3 100%); padding:20px; border-radius:10px; margin:30px 0;'>"
                        + "<p style='color:#8B5A2B; font-size:14px; margin:0 0 15px 0;'>Mã OTP của bạn:</p>"
                        + "<h3 style='color:#FFF; font-size:42px; background:linear-gradient(135deg, #8B5A2B 0%, #A0826D 100%); display:inline-block; padding:15px 35px; border-radius:8px; margin:0; letter-spacing:5px; font-family:monospace; font-weight:bold;'>" + otp + "</h3>"
                        + "</div>"
                        + "<p style='color:#666; font-size:14px; line-height:1.5; margin:25px 0;'><i>⏱️ Mã OTP này có hiệu lực trong <b>5 phút</b>. Vui lòng không chia sẻ mã này với bất kỳ ai.</i></p>"
                        + "<hr style='border:none; border-top:2px solid #E8D7C3; margin:30px 0;'>"
                        + "<p style='color:#777; font-size:13px; line-height:1.6; margin:20px 0;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>"
                        + "<p style='color:#8B5A2B; margin-top:30px; font-weight:600;'>Trân trọng,<br><b>Đội ngũ Woodify</b></p>"
                        + "</div>"
                        + "<div style='background:#f5ede3; padding:20px; text-align:center; border-top:1px solid #E8D7C3;'>"
                        + "<p style='color:#999; font-size:12px; margin:0;'>© 2026 Woodify. Tất cả quyền được bảo lưu.</p>"
                        + "</div>"
                        + "</div></body></html>";

            await _emailBackgroundQueue.QueueEmailAsync(new EmailMessage(email, subject, body));
            return true;
        }

        public async Task<bool> VerifyResetPasswordOtpAsync(string email, string otp)
        {
            return _otpStorage.TryGetValue(email, out var storedOtp)
            && storedOtp.Otp == otp
            && storedOtp.Expiry > DateTime.UtcNow;
        }

        public async Task<(bool Success, string? ResetToken)> VerifyResetPasswordOtpWithTokenAsync(string email, string otp)
        {
            if (_otpStorage.TryGetValue(email, out var storedOtp) && storedOtp.Otp == otp && storedOtp.Expiry > DateTime.UtcNow)
            {
                _otpStorage.Remove(email, out _);
                _verifiedEmails[email] = true;

                string resetToken = Guid.NewGuid().ToString();
                _resetTokens[resetToken] = (email, DateTime.UtcNow.AddMinutes(10));
                return (true, resetToken);
            }
            return (false, null);
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _unitOfWork.Accounts.GetByEmailAsync(email);
            if (user == null) return false;

            user.Password = newPassword;
            await _unitOfWork.Accounts.UpdateAsync(user);
            _otpStorage.Remove(email, out _);
            return true;
        }

        public async Task<bool> ResetPasswordWithTokenAsync(string resetToken, string newPassword)
        {
            if (_resetTokens.TryGetValue(resetToken, out var tokenData) && tokenData.Expiry > DateTime.UtcNow)
            {
                var email = tokenData.Email;
                var user = await _unitOfWork.Accounts.GetByEmailAsync(email);
                if (user == null) return false;

                user.Password = newPassword;
                await _unitOfWork.Accounts.UpdateAsync(user);
                _resetTokens.TryRemove(resetToken, out _);
                _verifiedEmails.TryRemove(email, out _);
                return true;
            }
            return false;
        }
        #endregion
    }
}