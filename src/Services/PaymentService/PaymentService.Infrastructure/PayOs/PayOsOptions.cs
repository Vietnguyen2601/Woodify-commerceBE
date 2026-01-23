namespace PaymentService.Infrastructure.PayOs;

/// <summary>
/// Cấu hình PayOS - sử dụng IOptions pattern
/// Các giá trị được load từ appsettings.json hoặc environment variables
/// </summary>
public class PayOsOptions
{
    /// <summary>
    /// Section name trong appsettings.json
    /// </summary>
    public const string SectionName = "PayOs";

    /// <summary>
    /// Client ID từ PayOS Dashboard
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// API Key từ PayOS Dashboard
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Checksum Key để verify webhook signature
    /// </summary>
    public string ChecksumKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL của PayOS API
    /// Sandbox: https://api-merchant.payos.vn
    /// Production: https://api-merchant.payos.vn
    /// </summary>
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";
}
