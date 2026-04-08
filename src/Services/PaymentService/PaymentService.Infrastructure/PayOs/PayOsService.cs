using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.PayOs.Models;

namespace PaymentService.Infrastructure.PayOs;

/// <summary>
/// PayOS Service - Xử lý tất cả các tương tác với PayOS API
/// Sử dụng HttpClientFactory để quản lý HttpClient
/// </summary>
public class PayOsService : IPayOsService
{
    private readonly HttpClient _httpClient;
    private readonly PayOsOptions _options;
    private readonly ILogger<PayOsService> _logger;

    public PayOsService(
        HttpClient httpClient,
        IOptions<PayOsOptions> options,
        ILogger<PayOsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configure HttpClient với base address và headers
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _options.ClientId);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
    }

    /// <summary>
    /// Tạo payment link PayOS
    /// </summary>
    public async Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(PayOsCreatePaymentInput input)
    {
        try
        {
            _logger.LogInformation("Creating PayOS payment link for orderCode: {OrderCode}, amount: {Amount}",
                input.OrderCode, input.Amount);

            // Generate signature cho request
            var dataToSign = $"amount={input.Amount}&cancelUrl={input.CancelUrl}&description={input.Description}&orderCode={input.OrderCode}&returnUrl={input.ReturnUrl}";
            var signature = GenerateSignature(dataToSign);

            _logger.LogDebug("PayOS request signature generated for data: {Data}", dataToSign);

            // Build request object
            var request = new PayOsCreatePaymentRequest
            {
                OrderCode = input.OrderCode,
                Amount = input.Amount,
                Description = input.Description,
                ReturnUrl = input.ReturnUrl,
                CancelUrl = input.CancelUrl,
                BuyerName = input.BuyerName,
                BuyerEmail = input.BuyerEmail,
                BuyerPhone = input.BuyerPhone,
                Signature = signature,
                Items = input.Items?.Select(i => new PayOsItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            // Gọi API PayOS
            var response = await _httpClient.PostAsJsonAsync("/v2/payment-requests", request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("PayOS API response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayOS API error. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);

                return new PayOsCreatePaymentResult
                {
                    IsSuccess = false,
                    Code = "99",
                    ErrorMessage = $"HTTP Error: {response.StatusCode}"
                };
            }

            var payOsResponse = JsonSerializer.Deserialize<PayOsCreatePaymentResponse>(responseContent);

            if (payOsResponse == null)
            {
                _logger.LogError("Failed to deserialize PayOS response");
                return new PayOsCreatePaymentResult
                {
                    IsSuccess = false,
                    Code = "99",
                    ErrorMessage = "Failed to parse response"
                };
            }

            if (payOsResponse.IsSuccess && payOsResponse.Data != null)
            {
                _logger.LogInformation("PayOS payment link created successfully. CheckoutUrl: {Url}",
                    payOsResponse.Data.CheckoutUrl);

                return new PayOsCreatePaymentResult
                {
                    IsSuccess = true,
                    Code = payOsResponse.Code,
                    CheckoutUrl = payOsResponse.Data.CheckoutUrl,
                    QrCodeUrl = payOsResponse.Data.QrCode,
                    OrderCode = payOsResponse.Data.OrderCode,
                    Amount = payOsResponse.Data.Amount,
                    Status = payOsResponse.Data.Status,
                    RawResponse = responseContent
                };
            }
            else
            {
                _logger.LogWarning("PayOS returned error code: {Code}, desc: {Desc}",
                    payOsResponse.Code, payOsResponse.Desc);

                return new PayOsCreatePaymentResult
                {
                    IsSuccess = false,
                    Code = payOsResponse.Code,
                    ErrorMessage = payOsResponse.Desc
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while calling PayOS API for orderCode: {OrderCode}",
                input.OrderCode);

            return new PayOsCreatePaymentResult
            {
                IsSuccess = false,
                Code = "99",
                ErrorMessage = $"Exception: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Query thông tin payment từ PayOS
    /// </summary>
    public async Task<PayOsPaymentInfoResult?> GetPaymentInfoAsync(long orderCode)
    {
        try
        {
            _logger.LogDebug("Querying PayOS payment info for orderCode: {OrderCode}", orderCode);

            var response = await _httpClient.GetAsync($"/v2/payment-requests/{orderCode}");
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("PayOS query response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayOS query error. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }

            var payOsResponse = JsonSerializer.Deserialize<PayOsPaymentInfoResponse>(responseContent);

            if (payOsResponse?.IsSuccess == true && payOsResponse.Data != null)
            {
                _logger.LogDebug("PayOS payment info retrieved. Status: {Status}",
                    payOsResponse.Data.Status);

                return new PayOsPaymentInfoResult
                {
                    IsSuccess = true,
                    OrderCode = payOsResponse.Data.OrderCode,
                    Amount = payOsResponse.Data.Amount,
                    AmountPaid = payOsResponse.Data.AmountPaid,
                    Status = payOsResponse.Data.Status,
                    CreatedAt = payOsResponse.Data.CreatedAt
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while querying PayOS for orderCode: {OrderCode}", orderCode);
            return null;
        }
    }

    /// <summary>
    /// Verify webhook signature từ PayOS
    /// Sử dụng HMAC-SHA256 với ChecksumKey
    /// </summary>
    public bool VerifyWebhookSignature(string data, string signature)
    {
        try
        {
            var expectedSignature = GenerateSignature(data);
            var isValid = string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug("Webhook signature verification. Expected: {Expected}, Received: {Received}, Valid: {Valid}",
                expectedSignature, signature, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Generate HMAC-SHA256 signature
    /// </summary>
    public string GenerateSignature(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
