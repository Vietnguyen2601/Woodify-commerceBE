using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Services;

/// <summary>
/// Payment Application Service - Business logic cho thanh toán
/// </summary>
public class PaymentAppService : IPaymentAppService
{
    private readonly IPayOsService _payOsService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentAppService> _logger;

    private const string PROVIDER_PAYOS = "PAYOS";

    public PaymentAppService(
        IPayOsService payOsService,
        IPaymentRepository paymentRepository,
        ILogger<PaymentAppService> logger)
    {
        _payOsService = payOsService;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Tạo link thanh toán PayOS
    /// </summary>
    public async Task<ServiceResult<CreatePaymentLinkResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment link for orderCode: {OrderCode}, amount: {Amount}",
                request.OrderCode, request.Amount);

            // Validate request
            if (request.OrderCode <= 0)
                return ServiceResult<CreatePaymentLinkResponse>.BadRequest("OrderCode phải là số nguyên dương");

            if (request.Amount <= 0)
                return ServiceResult<CreatePaymentLinkResponse>.BadRequest("Amount phải lớn hơn 0");

            if (string.IsNullOrWhiteSpace(request.ReturnUrl))
                return ServiceResult<CreatePaymentLinkResponse>.BadRequest("ReturnUrl không được để trống");

            if (string.IsNullOrWhiteSpace(request.CancelUrl))
                return ServiceResult<CreatePaymentLinkResponse>.BadRequest("CancelUrl không được để trống");

            // Check nếu orderCode đã tồn tại
            var existingPayment = await _paymentRepository.GetByProviderPaymentIdAsync(request.OrderCode.ToString());
            if (existingPayment != null)
            {
                return ServiceResult<CreatePaymentLinkResponse>.Conflict(
                    $"OrderCode {request.OrderCode} đã tồn tại");
            }

            // Tạo request cho PayOS
            var payOsRequest = new PayOsCreatePaymentInput
            {
                OrderCode = request.OrderCode,
                Amount = request.Amount,
                Description = request.Description.Length > 25
                    ? request.Description.Substring(0, 25)
                    : request.Description,
                ReturnUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                BuyerPhone = request.BuyerPhone
            };

            // Gọi PayOS API
            var payOsResponse = await _payOsService.CreatePaymentLinkAsync(payOsRequest);

            if (!payOsResponse.IsSuccess || string.IsNullOrEmpty(payOsResponse.CheckoutUrl))
            {
                _logger.LogError("PayOS API failed. Code: {Code}, Error: {Error}",
                    payOsResponse.Code, payOsResponse.ErrorMessage);

                return ServiceResult<CreatePaymentLinkResponse>.InternalServerError(
                    $"Tạo link thanh toán thất bại: {payOsResponse.ErrorMessage}");
            }

            // Lưu payment vào DB
            var payment = new Payment
            {
                OrderId = request.OrderId,
                AccountId = request.AccountId,
                Provider = PROVIDER_PAYOS,
                ProviderPaymentId = request.OrderCode.ToString(),
                AmountCents = request.Amount, // PayOS dùng VND, không phải cents
                Currency = "VND",
                Status = PaymentStatus.Created,
                ProviderResponse = payOsResponse.RawResponse
            };

            await _paymentRepository.CreateAsync(payment);

            _logger.LogInformation("Payment created successfully. PaymentId: {PaymentId}, CheckoutUrl: {Url}",
                payment.PaymentId, payOsResponse.CheckoutUrl);

            return ServiceResult<CreatePaymentLinkResponse>.Created(new CreatePaymentLinkResponse
            {
                PaymentId = payment.PaymentId,
                OrderCode = request.OrderCode,
                PaymentUrl = payOsResponse.CheckoutUrl,
                QrCodeUrl = payOsResponse.QrCodeUrl,
                Amount = request.Amount,
                Status = payOsResponse.Status ?? "PENDING"
            }, "Tạo link thanh toán thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating payment link for orderCode: {OrderCode}",
                request.OrderCode);

            return ServiceResult<CreatePaymentLinkResponse>.InternalServerError(
                $"Lỗi hệ thống: {ex.Message}");
        }
    }

    /// <summary>
    /// Query thông tin thanh toán theo orderCode
    /// </summary>
    public async Task<ServiceResult<PaymentInfoResponse>> GetPaymentByOrderCodeAsync(long orderCode)
    {
        try
        {
            _logger.LogInformation("Querying payment for orderCode: {OrderCode}", orderCode);

            // Tìm trong DB trước
            var payment = await _paymentRepository.GetByProviderPaymentIdAsync(orderCode.ToString());

            if (payment == null)
            {
                return ServiceResult<PaymentInfoResponse>.NotFound(
                    $"Payment not found for orderCode: {orderCode}");
            }

            // Query PayOS để lấy thông tin mới nhất
            var payOsInfo = await _payOsService.GetPaymentInfoAsync(orderCode);

            if (payOsInfo?.IsSuccess == true)
            {
                // Cập nhật status nếu có thay đổi
                var latestStatus = MapPayOsStatusToPaymentStatus(payOsInfo.Status);
                if (payment.Status != latestStatus)
                {
                    payment.Status = latestStatus;
                    await _paymentRepository.UpdateAsync(payment);
                }

                return ServiceResult<PaymentInfoResponse>.Success(new PaymentInfoResponse
                {
                    PaymentId = payment.PaymentId,
                    OrderCode = orderCode,
                    Amount = payOsInfo.Amount,
                    AmountPaid = payOsInfo.AmountPaid,
                    Status = payOsInfo.Status ?? payment.Status.ToString(),
                    Provider = PROVIDER_PAYOS,
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt
                });
            }

            // Trả về thông tin từ DB nếu không query được PayOS
            return ServiceResult<PaymentInfoResponse>.Success(new PaymentInfoResponse
            {
                PaymentId = payment.PaymentId,
                OrderCode = orderCode,
                Amount = (int)payment.AmountCents,
                AmountPaid = payment.Status == PaymentStatus.Succeeded ? (int)payment.AmountCents : 0,
                Status = payment.Status.ToString(),
                Provider = PROVIDER_PAYOS,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while querying payment for orderCode: {OrderCode}", orderCode);
            return ServiceResult<PaymentInfoResponse>.InternalServerError($"Lỗi hệ thống: {ex.Message}");
        }
    }

    /// <summary>
    /// Query thông tin thanh toán theo PaymentId
    /// </summary>
    public async Task<ServiceResult<PaymentInfoResponse>> GetPaymentByIdAsync(Guid paymentId)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);

            if (payment == null)
            {
                return ServiceResult<PaymentInfoResponse>.NotFound("Payment not found");
            }

            if (long.TryParse(payment.ProviderPaymentId, out var orderCode))
            {
                return await GetPaymentByOrderCodeAsync(orderCode);
            }

            return ServiceResult<PaymentInfoResponse>.Success(new PaymentInfoResponse
            {
                PaymentId = payment.PaymentId,
                OrderCode = 0,
                Amount = (int)payment.AmountCents,
                AmountPaid = payment.Status == PaymentStatus.Succeeded ? (int)payment.AmountCents : 0,
                Status = payment.Status.ToString(),
                Provider = payment.Provider ?? "",
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while querying payment by ID: {PaymentId}", paymentId);
            return ServiceResult<PaymentInfoResponse>.InternalServerError($"Lỗi hệ thống: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Map PayOS status string to PaymentStatus enum
    /// </summary>
    private static PaymentStatus MapPayOsStatusToPaymentStatus(string? payOsStatus)
    {
        return payOsStatus?.ToUpperInvariant() switch
        {
            "PAID" => PaymentStatus.Succeeded,
            "PENDING" => PaymentStatus.Processing,
            "CANCELLED" => PaymentStatus.Failed,
            "EXPIRED" => PaymentStatus.Failed,
            _ => PaymentStatus.Created
        };
    }

    #endregion
}
