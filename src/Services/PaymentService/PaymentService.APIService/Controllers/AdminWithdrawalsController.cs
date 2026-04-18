using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.APIService.Controllers;

[ApiController]
[Route("api/wallets/admin/withdrawals")]
public class AdminWithdrawalsController : ControllerBase
{
    private readonly IWithdrawalTicketService _withdrawals;
    private readonly ILogger<AdminWithdrawalsController> _logger;

    public AdminWithdrawalsController(
        IWithdrawalTicketService withdrawals,
        ILogger<AdminWithdrawalsController> logger)
    {
        _withdrawals = withdrawals;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketListResponse>>> List(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        WithdrawalTicketStatus? st = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<WithdrawalTicketStatus>(status, true, out var parsed))
            st = parsed;

        var result = await _withdrawals.ListAdminAsync(st, page, pageSize);
        return result.Status == 200 ? Ok(result) : StatusCode(result.Status, result);
    }

    /// <summary>
    /// Lấy toàn bộ ticket (không phân trang). Có thể lọc status. Trả về tối đa <paramref name="maxRows"/> bản ghi; <c>TotalCount</c> là tổng trong DB (có thể lớn hơn số phần tử trả về).
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketListResponse>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] int maxRows = 10_000)
    {
        WithdrawalTicketStatus? st = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<WithdrawalTicketStatus>(status, true, out var parsed))
            st = parsed;

        var result = await _withdrawals.ListAllAdminAsync(st, maxRows);
        return result.Status == 200 ? Ok(result) : StatusCode(result.Status, result);
    }

    [HttpPost("{ticketId:guid}/approve")]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketResponse>>> Approve(
        Guid ticketId, [FromBody] AdminReviewWithdrawalRequest request)
    {
        var result = await _withdrawals.ApproveAsync(ticketId, request);
        return Map(result);
    }

    [HttpPost("{ticketId:guid}/reject")]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketResponse>>> Reject(
        Guid ticketId, [FromBody] AdminReviewWithdrawalRequest request)
    {
        var result = await _withdrawals.RejectAsync(ticketId, request);
        return Map(result);
    }

    [HttpPost("{ticketId:guid}/mark-paid")]
    public async Task<ActionResult<ServiceResult<WithdrawalTicketResponse>>> MarkPaid(
        Guid ticketId, [FromBody] AdminReviewWithdrawalRequest request)
    {
        _logger.LogInformation("MarkPaid withdrawal {TicketId}", ticketId);
        var result = await _withdrawals.MarkPaidAsync(ticketId, request);
        return Map(result);
    }

    private ActionResult<ServiceResult<WithdrawalTicketResponse>> Map(ServiceResult<WithdrawalTicketResponse> result)
    {
        return result.Status switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            404 => NotFound(result),
            _ => StatusCode(result.Status, result)
        };
    }
}
