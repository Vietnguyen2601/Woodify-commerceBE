using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Results;

namespace PaymentService.APIService.Controllers;

/// <summary>
/// Controller xử lý thanh toán PayOS
/// </summary>
[ApiController]
[Route("api/payment/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly IPayOsWebhookHandler _webhookHandler;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentAppService paymentAppService,
        IPayOsWebhookHandler webhookHandler,
        ILogger<PaymentsController> logger)
    {
        _paymentAppService = paymentAppService;
        _webhookHandler = webhookHandler;
        _logger = logger;
    }

    /// <summary>
    /// Tạo Payment cho multi-order checkout
    /// Endpoint chính cho 3 phương thức: COD, Wallet, PayOS
    /// Schema:
    /// 1. Frontend gọi OrderService: POST /api/orders/CreateFromCart → nhận orderIds + totalAmount
    /// 2. Frontend gọi endpoint này: POST /api/payments/create → nhận paymentId + paymentUrl (nếu PayOS)
    /// 3. Tùy theo method:
    ///    - COD: Orders → CONFIRMED, Payment → PENDING
    ///    - WALLET: Deduct wallet, Orders → CONFIRMED, Payment → SUCCEEDED
    ///    - PAYOS: Payment → CREATED, return QR/link, orders chờ webhook
    /// </summary>
    /// <remarks>
    /// Sample requests:
    ///
    /// 1. COD Payment:
    ///     POST /api/payments/create
    ///     {
    ///         "orderIds": ["guid1", "guid2"],
    ///         "paymentMethod": "COD",
    ///         "accountId": "account-guid",
    ///         "TotalAmountVnd": 1500000
    ///     }
    ///
    /// 2. Wallet Payment (same structure, method="WALLET"):
    ///     {
    ///         "orderIds": [...],
    ///         "paymentMethod": "WALLET",
    ///         "accountId": "...",
    ///         "TotalAmountVnd": 1500000
    ///     }
    ///
    /// 3. PayOS Payment (with return URLs):
    ///     {
    ///         "orderIds": [...],
    ///         "paymentMethod": "PAYOS",
    ///         "accountId": "...",
    ///         "TotalAmountVnd": 1500000,
    ///         "returnUrl": "https://app.com/payment/success",
    ///         "cancelUrl": "https://app.com/payment/cancel"
    ///     }
    /// </remarks>

    /// <summary>
    /// Tạo link thanh toán PayOS (Legacy endpoint)
    /// Khuyên dùng POST /api/payments/create thay thế
    /// </summary>
    /// <remarks>
    /// 
    ///     POST /api/payments/payos/create
    ///     {
    ///         "orderCode": 123456789,
    ///         "amount": 50000,
    ///         "description": "Thanh toan don hang",
    ///         "returnUrl": "https://example.com/success",
    ///         "cancelUrl": "https://example.com/cancel"
    ///     }
    /// </remarks>
    /// <param name="request">Thông tin tạo payment</param>
    /// <returns>Payment link để redirect user</returns>
    [HttpPost("payos/create")]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentLinkResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentLinkResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentLinkResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentLinkResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<CreatePaymentLinkResponse>>> CreatePaymentLink(
        [FromBody] CreatePaymentLinkRequest request)
    {
        _logger.LogInformation("CreatePaymentLink request received for orderCode: {OrderCode}",
            request.OrderCode);

        var result = await _paymentAppService.CreatePaymentLinkAsync(request);

        return result.Status switch
        {
            201 => CreatedAtAction(nameof(GetPaymentByOrderCode),
                new { orderCode = request.OrderCode }, result),
            400 => BadRequest(result),
            409 => Conflict(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Webhook endpoint cho PayOS callback
    /// </summary>
    /// <remarks>
    /// PayOS sẽ gọi endpoint này khi có cập nhật trạng thái thanh toán.
    /// Cần đăng ký URL này trên PayOS Dashboard.
    /// </remarks>
    [HttpPost("payos/webhook")]
    [ProducesResponseType(typeof(PayOsWebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PayOsWebhookResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PayOsWebhookResponse>> HandleWebhook(
        [FromBody] PayOsWebhookData webhook)
    {
        _logger.LogInformation("PayOS webhook received. OrderCode: {OrderCode}, Status: {Status}",
            webhook.Data?.OrderCode, webhook.Data?.Status);

        var result = await _webhookHandler.HandleWebhookAsync(webhook);
        if (result.Code != "00")
        {
            _logger.LogWarning("Webhook handler returned error: {Code} - {Desc}", result.Code, result.Desc);
        }

        // Always return 200 so provider webhook validation/retries are not blocked by HTTP status.
        return Ok(result);
    }

    /// <summary>
    /// Query trạng thái thanh toán theo orderCode
    /// </summary>
    /// <param name="orderCode">Mã đơn hàng</param>
    /// <returns>Thông tin thanh toán</returns>
    [HttpGet("payos/{orderCode:long}")]
    [ProducesResponseType(typeof(ServiceResult<PaymentInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<PaymentInfoResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<PaymentInfoResponse>>> GetPaymentByOrderCode(long orderCode)
    {
        _logger.LogInformation("GetPaymentByOrderCode request for orderCode: {OrderCode}", orderCode);

        var result = await _paymentAppService.GetPaymentByOrderCodeAsync(orderCode);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Query trạng thái thanh toán theo PaymentId
    /// </summary>
    /// <param name="paymentId">ID thanh toán</param>
    /// <returns>Thông tin thanh toán</returns>
    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<PaymentInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<PaymentInfoResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<PaymentInfoResponse>>> GetPaymentById(Guid paymentId)
    {
        _logger.LogInformation("GetPaymentById request for paymentId: {PaymentId}", paymentId);

        var result = await _paymentAppService.GetPaymentByIdAsync(paymentId);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Tạo Payment cho multi-order checkout
    /// Hỗ trợ 3 phương thức: COD, WALLET, PAYOS
    /// Gọi sau khi CreateOrderFromCart thành công
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/payments/create
    ///     {
    ///         "orderIds": ["guid1", "guid2", "guid3"],
    ///         "paymentMethod": "COD",
    ///         "accountId": "550e8400-e29b-41d4-a716-446655440000"
    ///     }
    /// 
    /// Responses:
    /// - COD: status = "PENDING", orders sẽ CONFIRMED
    /// - WALLET: status = "SUCCEEDED", tiền đã deduct khỏi wallet
    /// - PAYOS: status = "CREATED", return paymentUrl + qrCodeUrl để user thanh toán
    /// </remarks>
    /// <param name="request">Thông tin tạo payment (orderIds, paymentMethod, accountId)</param>
    /// <returns>Payment info kèm đường thanh toán (für PayOS)</returns>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<CreatePaymentResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<CreatePaymentResponse>>> CreatePayment(
        [FromBody] CreatePaymentRequest request)
    {
        _logger.LogInformation("CreatePayment request received for {OrderCount} orders, method: {Method}",
            request.OrderIds.Count, request.PaymentMethod);

        var result = await _paymentAppService.CreatePaymentAsync(request);

        return result.Status switch
        {
            201 => CreatedAtAction(nameof(GetPaymentById),
                new { paymentId = result.Data?.PaymentId }, result),
            400 => BadRequest(result),
            _ => StatusCode(result.Status, result)
        };
    }
}

