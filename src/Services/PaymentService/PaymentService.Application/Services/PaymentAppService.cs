using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Helpers;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Shared.Events;
using Shared.Results;

namespace PaymentService.Application.Services;

/// <summary>
/// Payment Application Service - Business logic cho thanh toán
/// </summary>
public class PaymentAppService : IPaymentAppService
{
    private readonly IPayOsService _payOsService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOsWebhookHandler _payOsWebhookHandler;
    private readonly IPaymentEventsPublisher _paymentEventsPublisher;
    private readonly PaymentCallbackOptions _callbackOptions;
    private readonly ILogger<PaymentAppService> _logger;

    private const string PROVIDER_PAYOS = "PAYOS";
    private const string PROVIDER_WALLET = "WALLET";

    public PaymentAppService(
        IPayOsService payOsService,
        IPaymentRepository paymentRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        IPayOsWebhookHandler payOsWebhookHandler,
        IPaymentEventsPublisher paymentEventsPublisher,
        IOptions<PaymentCallbackOptions> callbackOptions,
        ILogger<PaymentAppService> logger)
    {
        _payOsService = payOsService;
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _payOsWebhookHandler = payOsWebhookHandler;
        _paymentEventsPublisher = paymentEventsPublisher;
        _callbackOptions = callbackOptions.Value;
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

            var returnUrl = ResolveCallbackUrl(request.ReturnUrl, _callbackOptions.ReturnUrl);
            var cancelUrl = ResolveCallbackUrl(request.CancelUrl, _callbackOptions.CancelUrl);
            if (returnUrl == null || cancelUrl == null)
            {
                return ServiceResult<CreatePaymentLinkResponse>.BadRequest(
                    "ReturnUrl/CancelUrl không hợp lệ. Vui lòng truyền absolute URL hợp lệ hoặc cấu hình PAYMENT_CALLBACK_RETURN_URL/PAYMENT_CALLBACK_CANCEL_URL.");
            }

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
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl,
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
                AmountVnd = request.Amount, // PayOS dùng VND, không phải cents
                Currency = "VND",
                Status = PaymentStatus.Created,
                ProviderResponse = payOsResponse.RawResponse,
                RelatedOrderIdsJson = request.OrderId.HasValue
                    ? JsonSerializer.Serialize(new List<Guid> { request.OrderId.Value })
                    : null
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
                var st = payOsInfo.Status?.ToUpperInvariant();
                if (st == "PAID" && payment.Status != PaymentStatus.Succeeded)
                {
                    var amountForWebhook = payOsInfo.Amount > 0
                        ? payOsInfo.Amount
                        : (payment.AmountVnd > int.MaxValue ? int.MaxValue : (int)payment.AmountVnd);
                    await _payOsWebhookHandler.HandleWebhookAsync(new PayOsWebhookData
                    {
                        Data = new PayOsWebhookPaymentData
                        {
                            OrderCode = orderCode,
                            Status = "PAID",
                            Description = "Synced from GetPaymentByOrderCode",
                            Amount = amountForWebhook,
                            Reference = ""
                        },
                        Signature = "auto-polling"
                    });
                    payment = await _paymentRepository.GetByProviderPaymentIdAsync(orderCode.ToString()) ?? payment;
                }
                else if (st == "CANCELLED" && payment.Status != PaymentStatus.Failed &&
                         payment.Status != PaymentStatus.Succeeded)
                {
                    await _payOsWebhookHandler.HandleWebhookAsync(new PayOsWebhookData
                    {
                        Data = new PayOsWebhookPaymentData
                        {
                            OrderCode = orderCode,
                            Status = "CANCELLED",
                            Description = "Synced from GetPaymentByOrderCode",
                            Amount = 0,
                            Reference = ""
                        },
                        Signature = "auto-polling"
                    });
                    payment = await _paymentRepository.GetByProviderPaymentIdAsync(orderCode.ToString()) ?? payment;
                }
                else
                {
                    var latestStatus = MapPayOsStatusToPaymentStatus(payOsInfo.Status);
                    if (payment.Status != latestStatus)
                    {
                        payment.Status = latestStatus;
                        await _paymentRepository.UpdateAsync(payment);
                    }
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
                Amount = (int)payment.AmountVnd,
                AmountPaid = payment.Status == PaymentStatus.Succeeded ? (int)payment.AmountVnd : 0,
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
                Amount = (int)payment.AmountVnd,
                AmountPaid = payment.Status == PaymentStatus.Succeeded ? (int)payment.AmountVnd : 0,
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

    /// <summary>
    /// Tạo Payment cho multi-order checkout
    /// Router method gọi 3 payment flows: COD, Wallet, PayOS
    /// </summary>
    public async Task<ServiceResult<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment. Method: {Method}, Orders: {OrderCount}, Amount: {Amount}",
                request.PaymentMethod, request.OrderIds.Count, request.TotalAmountVnd);

            // ===== VALIDATION =====
            // Validate orderIds
            if (request.OrderIds == null || !request.OrderIds.Any())
                return ServiceResult<CreatePaymentResponse>.BadRequest("OrderIds không được để trống");

            if (request.OrderIds.Count == 0)
                return ServiceResult<CreatePaymentResponse>.BadRequest("Phải có ít nhất 1 order");

            // Validate paymentMethod
            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                return ServiceResult<CreatePaymentResponse>.BadRequest("PaymentMethod không được để trống");

            // Validate accountId
            if (request.AccountId == Guid.Empty)
                return ServiceResult<CreatePaymentResponse>.BadRequest("AccountId không hợp lệ");

            // Validate TotalAmountVnd
            if (request.TotalAmountVnd <= 0)
                return ServiceResult<CreatePaymentResponse>.BadRequest("TotalAmountVnd phải lớn hơn 0");

            // ===== ROUTE BY PAYMENT METHOD =====
            return request.PaymentMethod.ToUpperInvariant() switch
            {
                "COD" => await CreateCODPaymentAsync(request),
                "WALLET" => await CreateWalletPaymentAsync(request),
                "PAYOS" => await CreatePayosPaymentAsync(request),
                _ => ServiceResult<CreatePaymentResponse>.BadRequest(
                    $"Phương thức thanh toán '{request.PaymentMethod}' không được hỗ trợ. " +
                    $"Chỉ hỗ trợ: COD, WALLET, PAYOS"
                )
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while creating payment. Account: {AccountId}, Orders: {OrderCount}",
                request.AccountId, request.OrderIds.Count);

            return ServiceResult<CreatePaymentResponse>.InternalServerError($"Lỗi hệ thống: {ex.Message}");
        }
    }

    /// <summary>
    /// COD Payment: Tạo Payment PENDING
    /// - Lưu payment với status = PENDING
    /// - Orders vẫn PENDING (sẽ confirm khi nhận hàng)
    /// - Không deduct tiền
    /// </summary>
    private async Task<ServiceResult<CreatePaymentResponse>> CreateCODPaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing COD payment. OrderIds: {OrderCount}, Amount: {Amount}",
                request.OrderIds.Count, request.TotalAmountVnd);

            // ===== Step 1: Validate amount =====
            if (request.TotalAmountVnd <= 0)
                return ServiceResult<CreatePaymentResponse>.BadRequest("TotalAmountVnd phải lớn hơn 0");

            // ===== Step 2: Tạo Payment record =====
            // Payment entity - giữ nguyên như yêu cầu, không thêm trường
            var payment = new Payment
            {
                OrderId = request.OrderIds.FirstOrDefault(), // Lưu order đầu tiên làm đại diện
                AccountId = request.AccountId,
                Provider = null, // COD không dùng provider
                AmountVnd = request.TotalAmountVnd, // === Lưu số tiền chính xác ===
                Status = PaymentStatus.Created, // Chờ thanh toán khi nhận hàng
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("COD payment created. PaymentId: {PaymentId}, Orders: {OrderCount}",
                payment.PaymentId, request.OrderIds.Count);

            return ServiceResult<CreatePaymentResponse>.Success(new CreatePaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderIds = request.OrderIds,
                TotalAmount = request.TotalAmountVnd,
                PaymentMethod = "COD",
                Status = "PENDING",
                Message = "Thanh toán COD đã được tạo. Chờ xác nhận khi nhận hàng"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating COD payment for account: {AccountId}", request.AccountId);
            return ServiceResult<CreatePaymentResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// Wallet Payment: Deduct từ wallet, Payment SUCCEEDED
    /// - Check wallet balance
    /// - Deduct amount từ wallet
    /// - Tạo WalletTransaction (DEBIT)
    /// - Return payment với remaining balance
    /// - OrderService: PaymentOrdersPaidEvent (RabbitMQ) → đơn chuyển COMPLETED
    /// </summary>
    private async Task<ServiceResult<CreatePaymentResponse>> CreateWalletPaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing Wallet payment. Account: {AccountId}, Amount: {Amount}",
                request.AccountId, request.TotalAmountVnd);

            // ===== Step 1: Validate amount =====
            if (request.TotalAmountVnd <= 0)
                return ServiceResult<CreatePaymentResponse>.BadRequest("TotalAmountVnd phải lớn hơn 0");

            // ===== Step 2: Fetch wallet =====
            var wallet = await _walletRepository.GetByAccountIdAsync(request.AccountId);
            if (wallet == null)
                return ServiceResult<CreatePaymentResponse>.NotFound("Không tìm thấy ví của tài khoản");

            // ===== Step 3: Check balance =====
            if (wallet.BalanceVnd < request.TotalAmountVnd)
            {
                _logger.LogWarning("Insufficient wallet balance. Account: {AccountId}, Balance: {Balance}, Amount: {Amount}",
                    request.AccountId, wallet.BalanceVnd, request.TotalAmountVnd);

                return ServiceResult<CreatePaymentResponse>.BadRequest(
                    $"Số dư không đủ. Cần: {request.TotalAmountVnd} VND, Hiện có: {wallet.BalanceVnd} VND"
                );
            }

            // ===== Step 4: Deduct from wallet =====
            wallet.BalanceVnd -= request.TotalAmountVnd;

            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                TxType = WalletTransactionType.Debit,
                AmountVnd = request.TotalAmountVnd,
                Note = $"Thanh toán {request.OrderIds.Count} đơn hàng",
                CreatedAt = DateTime.UtcNow
            };

            // ===== Step 5: Tạo Payment record (SUCCEEDED ngay) =====
            var payment = new Payment
            {
                OrderId = request.OrderIds.FirstOrDefault(),
                RelatedOrderIdsJson = JsonSerializer.Serialize(request.OrderIds),
                AccountId = request.AccountId,
                Provider = PROVIDER_WALLET,
                AmountVnd = request.TotalAmountVnd,
                Status = PaymentStatus.Succeeded,
                CreatedAt = DateTime.UtcNow
            };

            // ===== Step 6: Save all changes =====
            await _paymentRepository.CreateAsync(payment);
            await _walletRepository.UpdateAsync(wallet);
            await _walletRepository.AddTransactionAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            _paymentEventsPublisher.PublishPaymentOrdersPaid(new PaymentOrdersPaidEvent
            {
                PaymentId = payment.PaymentId,
                AccountId = request.AccountId,
                OrderIds = request.OrderIds,
                Provider = PROVIDER_WALLET,
                ProviderOrderCode = 0,
                AmountVnd = request.TotalAmountVnd,
                PaidAt = DateTime.UtcNow
            });

            _logger.LogInformation("Wallet payment succeeded. PaymentId: {PaymentId}, RemainingBalance: {Balance}",
                payment.PaymentId, wallet.BalanceVnd);

            return ServiceResult<CreatePaymentResponse>.Success(new CreatePaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderIds = request.OrderIds,
                TotalAmount = request.TotalAmountVnd,
                PaymentMethod = "WALLET",
                Status = "SUCCEEDED",
                RemainingBalance = wallet.BalanceVnd,
                Message = "Thanh toán ví điện tử thành công"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Wallet payment for account: {AccountId}", request.AccountId);
            return ServiceResult<CreatePaymentResponse>.InternalServerError($"Lỗi: {ex.Message}");
        }
    }

    /// <summary>
    /// PayOS Payment: Tạo link QR thanh toán, Payment CREATED
    /// - Generate unique order code
    /// - Call PayOS API để tạo payment link + QR
    /// - Lưu Payment với status = CREATED (chờ webhook)
    /// - Orders vẫn PENDING (chờ webhook confirm từ PayOS)
    /// </summary>
    private async Task<ServiceResult<CreatePaymentResponse>> CreatePayosPaymentAsync(CreatePaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing PayOS payment. OrderIds: {OrderCount}, Amount: {Amount}",
                request.OrderIds.Count, request.TotalAmountVnd);

            // ===== Step 1: Validate amount =====
            if (request.TotalAmountVnd <= 0)
                return ServiceResult<CreatePaymentResponse>.BadRequest("TotalAmountVnd phải lớn hơn 0");

            // ===== Step 2: Generate unique order code =====
            long orderCode = PaymentHelper.GenerateOrderCode(request.OrderIds);

            // ===== Step 3: Prepare PayOS request =====
            // PayOS Amount: integer VND (must match order TotalAmountVnd)
            int amountVnd = request.TotalAmountVnd > int.MaxValue
                ? int.MaxValue
                : (int)request.TotalAmountVnd;

            var returnUrl = request.ReturnUrl ?? _callbackOptions.ReturnUrl;
            var cancelUrl = request.CancelUrl ?? _callbackOptions.CancelUrl;
            if (string.IsNullOrWhiteSpace(returnUrl) || string.IsNullOrWhiteSpace(cancelUrl))
            {
                return ServiceResult<CreatePaymentResponse>.BadRequest(
                    "ReturnUrl/CancelUrl is missing. Please configure PAYMENT_CALLBACK_RETURN_URL and PAYMENT_CALLBACK_CANCEL_URL.");
            }
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out _) || !Uri.TryCreate(cancelUrl, UriKind.Absolute, out _))
            {
                return ServiceResult<CreatePaymentResponse>.BadRequest(
                    "ReturnUrl/CancelUrl is invalid. Please provide absolute URLs.");
            }

            // Description đơn giản: khách hàng chỉ cần biết số đơn hàng và tổng tiền
            var rawDesc = $"Thanh toan {request.OrderIds.Count} don hang Woodify";
            var payOsRequest = new PayOsCreatePaymentInput
            {
                OrderCode = orderCode,
                Amount = amountVnd, // PayOS dùng VND, không phải cents
                Description = rawDesc.Length > 25 ? rawDesc[..25] : rawDesc,
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
                // Items = null (không cần chi tiết từng shop - khách hàng chỉ quan tâm tổng tiền)
            };

            // ===== Step 4: Call PayOS API =====
            var payOsResponse = await _payOsService.CreatePaymentLinkAsync(payOsRequest);

            if (!payOsResponse.IsSuccess || string.IsNullOrEmpty(payOsResponse.CheckoutUrl))
            {
                _logger.LogError("PayOS API failed. Code: {Code}, Error: {Error}",
                    payOsResponse.Code, payOsResponse.ErrorMessage);

                return ServiceResult<CreatePaymentResponse>.InternalServerError(
                    $"Không thể tạo link thanh toán: {payOsResponse.ErrorMessage}"
                );
            }

            // ===== Step 5: Tạo Payment record với status = CREATED =====
            // Orders vẫn PENDING - chờ webhook từ PayOS confirm
            var payment = new Payment
            {
                OrderId = request.OrderIds.FirstOrDefault(),
                AccountId = request.AccountId,
                Provider = PROVIDER_PAYOS,
                ProviderPaymentId = orderCode.ToString(),
                AmountVnd = request.TotalAmountVnd,
                Status = PaymentStatus.Processing,
                ProviderResponse = payOsResponse.RawResponse,
                RelatedOrderIdsJson = JsonSerializer.Serialize(request.OrderIds),
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("PayOS payment created. PaymentId: {PaymentId}, OrderCode: {OrderCode}, Amount: {Amount}",
                payment.PaymentId, orderCode, amountVnd);

            return ServiceResult<CreatePaymentResponse>.Success(new CreatePaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderIds = request.OrderIds,
                TotalAmount = request.TotalAmountVnd,
                PaymentMethod = "PAYOS",
                Status = "CREATED",
                OrderCode = orderCode,
                PaymentUrl = payOsResponse.CheckoutUrl,
                QrCodeUrl = payOsResponse.QrCodeUrl,
                Message = "Quét mã QR hoặc truy cập link thanh toán để hoàn tất"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayOS payment for account: {AccountId}", request.AccountId);
            return ServiceResult<CreatePaymentResponse>.InternalServerError($"Lỗi: {ex.Message}");
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

    private static string? ResolveCallbackUrl(string? requestUrl, string? fallbackUrl)
    {
        if (!string.IsNullOrWhiteSpace(requestUrl) &&
            requestUrl != "string" &&
            Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
        {
            return requestUrl;
        }

        if (!string.IsNullOrWhiteSpace(fallbackUrl) &&
            Uri.TryCreate(fallbackUrl, UriKind.Absolute, out _))
        {
            return fallbackUrl;
        }

        return null;
    }

    #endregion
}
