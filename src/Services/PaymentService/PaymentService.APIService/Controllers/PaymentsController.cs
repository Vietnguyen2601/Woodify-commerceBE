using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Results;

namespace PaymentService.APIService.Controllers;

/// <summary>
/// Controller xử lý thanh toán PayOS
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentAppService _paymentAppService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentAppService paymentAppService,
        ILogger<PaymentsController> logger)
    {
        _paymentAppService = paymentAppService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo link thanh toán PayOS
    /// </summary>
    /// <remarks>
    /// Sample request:
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
    [ProducesResponseType(typeof(ServiceResult<WebhookProcessResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WebhookProcessResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<WebhookProcessResult>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ServiceResult<WebhookProcessResult>>> HandleWebhook(
        [FromBody] PayOsWebhookRequest webhook)
    {
        _logger.LogInformation("PayOS webhook received. Code: {Code}, OrderCode: {OrderCode}",
            webhook.Code, webhook.Data?.OrderCode);

        // Đọc raw body để log và verify signature
        string? rawBody = null;
        Request.Body.Position = 0;
        using (var reader = new StreamReader(Request.Body))
        {
            rawBody = await reader.ReadToEndAsync();
        }

        var result = await _paymentAppService.ProcessWebhookAsync(webhook, rawBody);

        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            401 => Unauthorized(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
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
}
