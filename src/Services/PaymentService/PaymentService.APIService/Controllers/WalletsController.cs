using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Results;

namespace PaymentService.APIService.Controllers;

/// <summary>
/// Controller quản lý Wallet (ví điện tử)
/// </summary>
[ApiController]
[Route("api/wallets")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IPaymentAppService _paymentAppService;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletService walletService,
        IPaymentAppService paymentAppService,
        ILogger<WalletsController> logger)
    {
        _walletService = walletService;
        _paymentAppService = paymentAppService;
        _logger = logger;
    }

    /// <summary>
    /// Tạo ví mới cho Account
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceResult<WalletResponse>>> CreateWallet(
        [FromBody] CreateWalletRequest request)
    {
        _logger.LogInformation("CreateWallet request for accountId: {AccountId}", request.AccountId);

        var result = await _walletService.CreateWalletAsync(request);

        return result.Status switch
        {
            201 => CreatedAtAction(nameof(GetWallet), new { walletId = result.Data?.WalletId }, result),
            400 => BadRequest(result),
            409 => Conflict(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Lấy thông tin ví theo ID
    /// </summary>
    [HttpGet("{walletId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<WalletResponse>>> GetWallet(Guid walletId)
    {
        _logger.LogInformation("GetWallet request for walletId: {WalletId}", walletId);

        var result = await _walletService.GetWalletAsync(walletId);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Lấy thông tin ví theo AccountId
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<WalletResponse>>> GetWalletByAccount(Guid accountId)
    {
        _logger.LogInformation("GetWalletByAccount request for accountId: {AccountId}", accountId);

        var result = await _walletService.GetWalletByAccountAsync(accountId);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Nạp tiền vào ví
    /// </summary>
    [HttpPost("{walletId:guid}/credit")]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<WalletTransactionResult>>> CreditWallet(
        Guid walletId, [FromBody] CreditWalletRequest request)
    {
        _logger.LogInformation("CreditWallet request for walletId: {WalletId}, amount: {Amount}",
            walletId, request.Amount);

        var result = await _walletService.CreditAsync(walletId, request);

        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Trừ tiền từ ví
    /// </summary>
    [HttpPost("{walletId:guid}/debit")]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<WalletTransactionResult>>> DebitWallet(
        Guid walletId, [FromBody] DebitWalletRequest request)
    {
        _logger.LogInformation("DebitWallet request for walletId: {WalletId}, amount: {Amount}",
            walletId, request.Amount);

        var result = await _walletService.DebitAsync(walletId, request);

        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Lấy lịch sử giao dịch ví
    /// </summary>
    [HttpGet("{walletId:guid}/transactions")]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionListResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceResult<WalletTransactionListResponse>>> GetTransactions(
        Guid walletId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation(
            "GetTransactions request for walletId: {WalletId}, page: {Page}, pageSize: {PageSize}",
            walletId, page, pageSize);

        var result = await _walletService.GetTransactionsAsync(walletId, page, pageSize);

        return result.Status switch
        {
            200 => Ok(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }

    /// <summary>
    /// Nạp tiền vào ví thông qua thanh toán PayOS
    /// </summary>
    /// <remarks>
    /// Flow:
    /// 1. API tạo link thanh toán PayOS
    /// 2. User được redirect đến PayOS để thanh toán
    /// 3. Sau khi thanh toán, PayOS gọi webhook
    /// 4. Webhook tự động nạp tiền vào ví
    /// 
    /// Sample request:
    /// 
    ///     POST /api/wallets/topup
    ///     {
    ///         "accountId": "550e8400-e29b-41d4-a716-446655440000",
    ///         "amount": 100000,
    ///         "returnUrl": "https://app.example.com/wallet/topup-success",
    ///         "cancelUrl": "https://app.example.com/wallet/topup-cancel",
    ///         "buyerName": "Nguyễn Văn A",
    ///         "buyerEmail": "user@example.com",
    ///         "buyerPhone": "0909123456"
    ///     }
    /// </remarks>
    [HttpPost("topup")]
    [ProducesResponseType(typeof(ServiceResult<TopUpWalletWithPaymentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResult<TopUpWalletWithPaymentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<TopUpWalletWithPaymentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ServiceResult<TopUpWalletWithPaymentResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<TopUpWalletWithPaymentResponse>>> TopUpWithPayment(
        [FromBody] TopUpWalletWithPaymentRequest request)
    {
        _logger.LogInformation(
            "TopUpWithPayment request for accountId: {AccountId}, amount: {Amount}",
            request.AccountId, request.Amount);

        try
        {
            // 1. Kiểm tra ví tồn tại
            var walletResult = await _walletService.GetWalletByAccountAsync(request.AccountId);
            if (walletResult.Status != 200 || walletResult.Data == null)
            {
                _logger.LogWarning("Wallet not found for accountId: {AccountId}", request.AccountId);
                return NotFound(new ServiceResult<TopUpWalletWithPaymentResponse>
                {
                    Status = 404,
                    Message = "Wallet not found"
                });
            }

            var wallet = walletResult.Data;

            // 2. Tạo link thanh toán PayOS
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Amount = (int)request.Amount,
                Description = $"Nạp tiền vào ví",
                ReturnUrl = request.ReturnUrl,
                CancelUrl = request.CancelUrl,
                AccountId = request.AccountId,
                BuyerName = request.BuyerName,
                BuyerEmail = request.BuyerEmail,
                BuyerPhone = request.BuyerPhone
            };

            var paymentResult = await _paymentAppService.CreatePaymentLinkAsync(paymentRequest);

            if (paymentResult.Status != 201 || paymentResult.Data == null)
            {
                _logger.LogError("Failed to create payment link for accountId: {AccountId}", request.AccountId);
                return StatusCode(500, new ServiceResult<TopUpWalletWithPaymentResponse>
                {
                    Status = 500,
                    Message = "Failed to create payment link"
                });
            }

            // 3. Trả về response
            var response = new TopUpWalletWithPaymentResponse
            {
                PaymentId = paymentResult.Data.PaymentId,
                WalletId = wallet.WalletId,
                Amount = request.Amount,
                PaymentUrl = paymentResult.Data.PaymentUrl,
                QrCodeUrl = paymentResult.Data.QrCodeUrl,
                Status = "PENDING"
            };

            _logger.LogInformation(
                "TopUp payment created successfully. PaymentId: {PaymentId}, WalletId: {WalletId}",
                response.PaymentId, response.WalletId);

            var successResult = new ServiceResult<TopUpWalletWithPaymentResponse>
            {
                Status = 201,
                Data = response,
                Message = "Top-up payment initiated. Please complete payment on PayOS."
            };

            return CreatedAtAction(nameof(GetWallet),
                new { walletId = wallet.WalletId },
                successResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating top-up payment for accountId: {AccountId}", request.AccountId);
            return StatusCode(500, new ServiceResult<TopUpWalletWithPaymentResponse>
            {
                Status = 500,
                Message = "Internal server error"
            });
        }
    }
}
