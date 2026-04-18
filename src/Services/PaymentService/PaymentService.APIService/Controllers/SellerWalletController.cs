using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Results;

namespace PaymentService.APIService.Controllers;

[ApiController]
[Route("api/wallets/seller")]
public class SellerWalletController : ControllerBase
{
    private readonly ISellerWalletReadService _sellerWallet;
    private readonly IWithdrawalTicketService _withdrawals;
    private readonly ILogger<SellerWalletController> _logger;

    public SellerWalletController(
        ISellerWalletReadService sellerWallet,
        IWithdrawalTicketService withdrawals,
        ILogger<SellerWalletController> logger)
    {
        _sellerWallet = sellerWallet;
        _withdrawals = withdrawals;
        _logger = logger;
    }

    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(ServiceResult<WalletResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<WalletResponse>>> GetSellerWallet(Guid accountId)
    {
        var result = await _sellerWallet.GetOrCreateSellerWalletAsync(accountId);
        return result.Status switch
        {
            200 => Ok(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpGet("account/{accountId:guid}/transactions")]
    [ProducesResponseType(typeof(ServiceResult<WalletTransactionListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<WalletTransactionListResponse>>> GetTransactions(
        Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sellerWallet.GetSellerTransactionsAsync(accountId, page, pageSize);
        return result.Status switch
        {
            200 => Ok(result),
            _ => StatusCode(result.Status, result)
        };
    }

    [HttpPost("withdrawals")]
    [ProducesResponseType(typeof(ServiceResult<WithdrawalTicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketResponse>>> CreateWithdrawal(
        [FromBody] CreateSellerWithdrawalRequest request)
    {
        _logger.LogInformation(
            "CreateWithdrawal seller={Seller} shop={Shop} amount={Amount}",
            request.SellerAccountId, request.ShopId, request.AmountVnd);

        var result = await _withdrawals.CreateAsync(request);
        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            409 => Conflict(result),
            _ => StatusCode(result.Status, result)
        };
    }
}
