using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using Shared.Results;

namespace PaymentService.Application.Interfaces;

public interface IWithdrawalTicketService
{
    Task<ServiceResult<WithdrawalTicketResponse>> CreateAsync(CreateSellerWithdrawalRequest request);
    Task<ServiceResult<WithdrawalTicketListResponse>> ListAdminAsync(WithdrawalTicketStatus? status, int page, int pageSize);

    /// <summary>Danh sách đầy đủ (không phân trang), tối đa <paramref name="maxRows"/> dòng — dùng khi DB vừa phải.</summary>
    Task<ServiceResult<WithdrawalTicketListResponse>> ListAllAdminAsync(
        WithdrawalTicketStatus? status,
        int maxRows = 10_000);
    /// <summary>Duyệt ticket và trừ ví ngay.</summary>
    Task<ServiceResult<WithdrawalTicketResponse>> ApproveAsync(Guid ticketId, AdminReviewWithdrawalRequest request);
    Task<ServiceResult<WithdrawalTicketResponse>> RejectAsync(Guid ticketId, AdminReviewWithdrawalRequest request);
    /// <summary>Chỉ cập nhật trạng thái Paid sau khi CK tay (ví đã trừ lúc Approve).</summary>
    Task<ServiceResult<WithdrawalTicketResponse>> MarkPaidAsync(Guid ticketId, AdminReviewWithdrawalRequest request);
}
