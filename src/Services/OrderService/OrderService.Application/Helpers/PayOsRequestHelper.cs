using PaymentService.Application.Interfaces;

namespace OrderService.Application.Helpers;

/// <summary>
/// Helper class để tạo PayOS request với các thông số tối ưu
/// </summary>
public static class PayOsRequestHelper
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Generate random orderCode (3 digits từ 100-999)
    /// </summary>
    public static long GenerateOrderCode()
    {
        return _random.Next(100, 1000);
    }

    /// <summary>
    /// Generate random email dạng xxxxxx@gmail.com
    /// </summary>
    public static string GenerateRandomEmail()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var randomPart = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[_random.Next(chars.Length)])
            .ToArray());

        return $"{randomPart}@gmail.com";
    }

    /// <summary>
    /// Generate description cho payment (tối đa 25 kí tự)
    /// </summary>
    public static string GenerateDescription(Guid orderId, Guid shopId)
    {
        const int maxLength = 25;
        var description = $"Thanh toán đơn hàng {orderId:N}";

        // Truncate nếu vượt quá 25 kí tự
        if (description.Length > maxLength)
        {
            description = description.Substring(0, maxLength - 3) + "...";
        }

        return description;
    }

    /// <summary>
    /// Sanitize buyerName: remove special chars, truncate to 50 chars
    /// </summary>
    public static string SanitizeBuyerName(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Khách hàng";

        // Remove leading/trailing whitespace
        var cleaned = input.Trim();

        // Truncate nếu quá dài
        if (cleaned.Length > 50)
            cleaned = cleaned.Substring(0, 50);

        return cleaned;
    }

    /// <summary>
    /// Sanitize phone number: remove non-digits, keep only 10-11 digits
    /// </summary>
    public static string SanitizeBuyerPhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "0000000000"; // Default placeholder

        // Remove all non-digit characters
        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(input, @"\D", "");

        // Validate length (Vietnamese phone: 10-11 digits)
        if (digitsOnly.Length < 10)
            return "0000000000";

        if (digitsOnly.Length > 11)
            digitsOnly = digitsOnly.Substring(0, 11);

        return digitsOnly;
    }

    /// <summary>
    /// Sanitize URLs: use defaults if empty
    /// </summary>
    public static string NormalizeUrl(string? input, string defaultUrl)
    {
        return string.IsNullOrWhiteSpace(input) ? defaultUrl : input.Trim();
    }

    /// <summary>
    /// Tạo PayOS request input từ order với validation tối ưu
    /// </summary>
    public static PayOsCreatePaymentInput CreatePayOsRequest(
        Guid orderId,
        Guid shopId,
        double TotalAmountVnd,
        string? buyerName,
        string? buyerEmail,
        string? buyerPhone,
        string returnUrl,
        string cancelUrl)
    {
        // Convert từ cents (VND) thành đơn vị PayOS
        int amountInVnd = (int)TotalAmountVnd;

        // Default URLs (fallback nếu không được cung cấp)
        const string defaultReturnUrl = "http://localhost:3000/payment/success";
        const string defaultCancelUrl = "http://localhost:3000/payment/cancel";

        var request = new PayOsCreatePaymentInput
        {
            OrderCode = GenerateOrderCode(),
            Amount = amountInVnd,
            Description = GenerateDescription(orderId, shopId),
            ReturnUrl = NormalizeUrl(returnUrl, defaultReturnUrl),
            CancelUrl = NormalizeUrl(cancelUrl, defaultCancelUrl),
            BuyerName = SanitizeBuyerName(buyerName),
            BuyerEmail = buyerEmail ?? GenerateRandomEmail(),
            BuyerPhone = SanitizeBuyerPhone(buyerPhone)
        };

        return request;
    }
}
