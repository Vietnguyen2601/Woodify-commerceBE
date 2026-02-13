namespace IdentityService.Application.Constants;

public static class AuthMessages
{
    // Validation
    public const string InvalidData = "Dữ liệu không hợp lệ";

    // OTP - Registration
    public const string OtpSentSuccess = "OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
    public const string EmailAlreadyRegistered = "Email đã được đăng ký. Vui lòng sử dụng email khác.";
    public const string OtpVerifySuccess = "Xác minh OTP thành công. Bạn có thể tiếp tục đăng ký.";
    public const string OtpInvalidOrExpired = "OTP không hợp lệ hoặc đã hết hạn";

    // Email Verification
    public const string EmailVerified = "Email đã được xác minh";
    public const string EmailNotVerified = "Email chưa được xác minh";

    // Register
    public const string RegisterSuccess = "Đăng ký tài khoản thành công.";
    public const string OtpNotVerifiedForRegister = "Vui lòng xác minh OTP trước khi đăng ký tài khoản.";
    public const string UsernameAlreadyExists = "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.";
    public const string PasswordMismatch = "Mật khẩu xác nhận không khớp.";

    // Login
    public const string LoginSuccess = "Đăng nhập thành công.";
    public const string InvalidCredentials = "Email hoặc mật khẩu không đúng.";
    public const string AccountNotActive = "Tài khoản chưa được kích hoạt.";

    // Forgot Password
    public const string ResetOtpSentSuccess = "OTP đặt lại mật khẩu đã được gửi đến email của bạn.";
    public const string EmailNotFound = "Email không tồn tại trong hệ thống";
    public const string ResetOtpVerifySuccess = "Xác minh OTP thành công. Vui lòng sử dụng reset token để đặt lại mật khẩu.";

    // Reset Password
    public const string ResetPasswordSuccess = "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới.";
    public const string ResetTokenInvalidOrExpired = "Reset token không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu OTP mới.";
    public const string OtpNotVerified = "Vui lòng xác minh OTP trước khi đặt lại mật khẩu";
    public const string AccountNotFound = "Không tìm thấy tài khoản với email này";

    // Refresh Token
    public const string RefreshTokenSuccess = "Làm mới token thành công.";
    public const string RefreshTokenInvalid = "Refresh token không hợp lệ hoặc đã hết hạn.";
}
