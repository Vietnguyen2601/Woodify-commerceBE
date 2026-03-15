using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Services;

/// <summary>
/// Service xử lý nạp tiền vào ví qua Payment Gateway
/// </summary>
public class WalletTopUpService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPayOsService _payOsService;
    private readonly IPaymentPollingTrigger _pollingTrigger;
    private readonly PaymentCallbackOptions _callbackOptions;
    private readonly ILogger<WalletTopUpService> _logger;

    private const string PROVIDER_PAYOS = "PAYOS";
    private const long MIN_AMOUNT = 10_000;      // 10,000 VND
    private const long MAX_AMOUNT = 1_000_000_000; // 1 tỷ VND

    public WalletTopUpService(
        IWalletRepository walletRepository,
        IPaymentRepository paymentRepository,
        IPayOsService payOsService,
        IPaymentPollingTrigger pollingTrigger,
        IOptions<PaymentCallbackOptions> callbackOptions,
        ILogger<WalletTopUpService> logger)
    {
        _walletRepository = walletRepository;
        _paymentRepository = paymentRepository;
        _payOsService = payOsService;
        _pollingTrigger = pollingTrigger;
        _callbackOptions = callbackOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Nạp tiền vào ví thông qua Payment Gateway
    /// </summary>
    public async Task<ServiceResult<WalletTopUpResponse>> TopUpAsync(WalletTopUpRequest request)
    {
        try
        {
            _logger.LogInformation("WalletTopUp request - WalletId: {WalletId}, Amount: {Amount}, Method: {Method}",
                request.WalletId, request.Amount, request.Method);

            // Validate request
            var validation = ValidateTopUpRequest(request);
            if (!validation.IsValid)
            {
                _logger.LogWarning("WalletTopUp validation failed: {Error}", validation.ErrorMessage);
                return ServiceResult<WalletTopUpResponse>.BadRequest(validation.ErrorMessage);
            }

            // Get wallet
            var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found: {WalletId}", request.WalletId);
                return ServiceResult<WalletTopUpResponse>.NotFound("Ví không tồn tại");
            }

            // Check wallet status
            if (wallet.Status != WalletStatus.Active)
            {
                _logger.LogWarning("Wallet not active: {WalletId}, Status: {Status}", request.WalletId, wallet.Status);
                return ServiceResult<WalletTopUpResponse>.BadRequest($"Ví không hoạt động (Status: {wallet.Status})");
            }

            // Route to payment method handler
            return request.Method.ToUpper() switch
            {
                "PAYOS" => await HandlePayOsTopUpAsync(request, wallet),
                "MOMO" => await HandleMoMoTopUpAsync(request, wallet),
                "VNPAY" => await HandleVNPayTopUpAsync(request, wallet),
                _ => ServiceResult<WalletTopUpResponse>.BadRequest($"Phương thức {request.Method} không được hỗ trợ")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WalletTopUpAsync");
            return ServiceResult<WalletTopUpResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Xác nhận nạp tiền thành công từ webhook
    /// </summary>
    public async Task<ServiceResult<bool>> ConfirmTopUpAsync(Guid paymentId, string status)
    {
        try
        {
            _logger.LogInformation("Confirming top-up for PaymentId: {PaymentId}, Status: {Status}", paymentId, status);

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
                return ServiceResult<bool>.NotFound("Thanh toán không tồn tại");
            }

            // Update payment status
            if (status.Equals("SUCCEEDED", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Succeeded;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                _logger.LogInformation("Payment confirmed: {PaymentId}", paymentId);
                return ServiceResult<bool>.Success(true, "Xác nhận nạp tiền thành công");
            }
            else if (status.Equals("FAILED", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                _logger.LogWarning("Payment failed: {PaymentId}", paymentId);
                return ServiceResult<bool>.BadRequest("Nạp tiền thất bại");
            }

            return ServiceResult<bool>.BadRequest($"Status không hợp lệ: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming top-up");
            return ServiceResult<bool>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    #region Private Handler Methods

    /// <summary>
    /// Xử lý nạp tiền qua PayOS
    /// </summary>
    private async Task<ServiceResult<WalletTopUpResponse>> HandlePayOsTopUpAsync(
        WalletTopUpRequest request, Wallet wallet)
    {
        try
        {
            _logger.LogInformation("Processing PayOS top-up for wallet: {WalletId}, amount: {Amount}",
                request.WalletId, request.Amount);

            // Use URLs from config
            var returnUrl = _callbackOptions.ReturnUrl;
            var cancelUrl = _callbackOptions.CancelUrl;

            // Generate unique order code
            var orderCode = GenerateOrderCode();

            // Create PayOS request
            var payOsRequest = new PayOsCreatePaymentInput
            {
                OrderCode = orderCode,
                Amount = (int)request.Amount,
                Description = "Nap tien vi", // Max 25 chars
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            _logger.LogInformation("Calling PayOS API for orderCode: {OrderCode}", orderCode);

            // Call PayOS API
            var payOsResponse = await _payOsService.CreatePaymentLinkAsync(payOsRequest);

            if (!payOsResponse.IsSuccess)
            {
                _logger.LogError("PayOS API failed: {Code} - {Error}", payOsResponse.Code, payOsResponse.ErrorMessage);
                return ServiceResult<WalletTopUpResponse>.InternalServerError(
                    $"Tạo link thanh toán thất bại: {payOsResponse.ErrorMessage}");
            }

            // Create Payment record
            var payment = new Payment
            {
                AccountId = wallet.AccountId,
                Provider = PROVIDER_PAYOS,
                ProviderPaymentId = orderCode.ToString(),
                AmountCents = request.Amount,
                Currency = "VND",
                Status = PaymentStatus.Processing,
                ProviderResponse = payOsResponse.RawResponse,
                CreatedAt = DateTime.UtcNow
            };

            var savedPayment = await _paymentRepository.CreateAsync(payment);

            _logger.LogInformation("Payment record created: {PaymentId}, OrderCode: {OrderCode}",
                savedPayment.PaymentId, orderCode);

            // Trigger polling session để auto-detect khi user thanh toán xong
            _pollingTrigger.Trigger();

            // Return response
            return ServiceResult<WalletTopUpResponse>.Created(new WalletTopUpResponse
            {
                PaymentId = savedPayment.PaymentId,
                OrderCode = orderCode,
                Amount = request.Amount,
                Status = "PENDING",
                PaymentUrl = payOsResponse.CheckoutUrl,
                QrCodeUrl = payOsResponse.QrCodeUrl,
                Fee = 0,
                CreatedAt = DateTime.UtcNow,
                Message = "Vui lòng chuyển hướng user đến PaymentUrl để thanh toán"
            }, "Tạo link nạp tiền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PayOS top-up handler");
            throw;
        }
    }

    /// <summary>
    /// Xử lý nạp tiền qua MoMo (placeholder)
    /// </summary>
    private async Task<ServiceResult<WalletTopUpResponse>> HandleMoMoTopUpAsync(
        WalletTopUpRequest request, Wallet wallet)
    {
        _logger.LogInformation("MoMo top-up not implemented yet");
        return await Task.FromResult(
            ServiceResult<WalletTopUpResponse>.InternalServerError("Phương thức MoMo chưa được hỗ trợ"));
    }

    /// <summary>
    /// Xử lý nạp tiền qua VNPay (placeholder)
    /// </summary>
    private async Task<ServiceResult<WalletTopUpResponse>> HandleVNPayTopUpAsync(
        WalletTopUpRequest request, Wallet wallet)
    {
        _logger.LogInformation("VNPay top-up not implemented yet");
        return await Task.FromResult(
            ServiceResult<WalletTopUpResponse>.InternalServerError("Phương thức VNPay chưa được hỗ trợ"));
    }

    #endregion

    #region Private Validation & Utility

    /// <summary>
    /// Validate top-up request
    /// </summary>
    private ValidationResult ValidateTopUpRequest(WalletTopUpRequest request)
    {
        if (request.WalletId == Guid.Empty)
            return ValidationResult.Invalid("WalletId không hợp lệ");

        if (request.Amount < MIN_AMOUNT)
            return ValidationResult.Invalid($"Số tiền tối thiểu là {MIN_AMOUNT:N0} VND");

        if (request.Amount > MAX_AMOUNT)
            return ValidationResult.Invalid($"Số tiền tối đa là {MAX_AMOUNT:N0} VND");

        if (string.IsNullOrWhiteSpace(request.Method))
            return ValidationResult.Invalid("Phương thức thanh toán không được để trống");

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Generate unique order code
    /// </summary>
    private long GenerateOrderCode()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000000000;
        var random = new Random().Next(10000, 99999);
        return timestamp * 100000 + random;
    }

    #endregion

    #region Helper Classes

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string msg) => new() { IsValid = false, ErrorMessage = msg };
    }

    #endregion
}
