namespace PaymentService.Application.DTOs;

public class PayOsOptions
{
    public const string SectionName = "PayOs";
    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";
}

public class PaymentCallbackOptions
{
    public string ReturnUrl { get; set; } = null!;
    public string CancelUrl { get; set; } = null!;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ReturnUrl) &&
               !string.IsNullOrWhiteSpace(CancelUrl) &&
               Uri.TryCreate(ReturnUrl, UriKind.Absolute, out _) &&
               Uri.TryCreate(CancelUrl, UriKind.Absolute, out _);
    }
}
