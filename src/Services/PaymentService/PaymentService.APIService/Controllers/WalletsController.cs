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
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletService walletService,
        ILogger<WalletsController> logger)
    {
        _walletService = walletService;
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
    /// Nạp tiền vào ví qua Payment Gateway (PayOS, MoMo, VNPay,...)
    /// </summary>
    [HttpPost("topup")]
    [ProducesResponseType(typeof(ServiceResult<WalletTopUpResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ServiceResult<WalletTopUpResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResult<WalletTopUpResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ServiceResult<WalletTopUpResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResult<WalletTopUpResponse>>> TopUpWallet(
        [FromBody] WalletTopUpRequest request)
    {
        _logger.LogInformation("TopUpWallet request - WalletId: {WalletId}, Amount: {Amount}, Method: {Method}",
            request.WalletId, request.Amount, request.Method);

        var result = await _walletService.TopUpAsync(request);

        return result.Status switch
        {
            201 => CreatedAtAction(nameof(GetWallet), new { walletId = request.WalletId }, result),
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

}
