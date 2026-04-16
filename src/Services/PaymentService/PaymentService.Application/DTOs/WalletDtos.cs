using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request tạo ví mới
/// </summary>
public class CreateWalletRequest
{
    /// <summary>
    /// ID của Account (bắt buộc)
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Loại tiền tệ (mặc định VND)
    /// </summary>
    public string Currency { get; set; } = "VND";
}

/// <summary>
/// Request nạp tiền vào ví
/// </summary>
public class CreditWalletRequest
{
    /// <summary>
    /// Số tiền nạp (VND)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// ID Payment liên quan (nếu có)
    /// </summary>
    public Guid? RelatedPaymentId { get; set; }

    /// <summary>
    /// ID Order liên quan (nếu có)
    /// </summary>
    public Guid? RelatedOrderId { get; set; }
}

/// <summary>
/// Request trừ tiền từ ví
/// </summary>
public class DebitWalletRequest
{
    /// <summary>
    /// Số tiền trừ (VND)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Ghi chú
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// ID Order liên quan (nếu có)
    /// </summary>
    public Guid? RelatedOrderId { get; set; }

    /// <summary>
    /// ID Payment liên quan (nếu có)
    /// </summary>
    public Guid? RelatedPaymentId { get; set; }
}

/// <summary>
/// Request nạp tiền vào ví qua Payment Gateway
/// </summary>
public class WalletTopUpRequest
{
    /// <summary>
    /// ID Ví (bắt buộc)
    /// </summary>
    public Guid WalletId { get; set; }

    /// <summary>
    /// Số tiền nạp (VND, bắt buộc)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Phương thức thanh toán: PayOS, MoMo, VNPay (bắt buộc)
    /// </summary>
    public string Method { get; set; } = string.Empty;
}

#endregion

#region Response DTOs

/// <summary>
/// Response thông tin ví
/// </summary>
public class WalletResponse
{
    public Guid WalletId { get; set; }
    public Guid AccountId { get; set; }

    /// <summary>
    /// Số dư (VND)
    /// </summary>
    public long Balance { get; set; }

    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Response giao dịch ví
/// </summary>
public class WalletTransactionResponse
{
    public Guid TransactionId { get; set; }
    public Guid WalletId { get; set; }

    /// <summary>
    /// Loại giao dịch: Credit, Debit, Hold, Release
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Số tiền (VND)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Số dư trước giao dịch
    /// </summary>
    public long? BalanceBefore { get; set; }

    /// <summary>
    /// Số dư sau giao dịch
    /// </summary>
    public long? BalanceAfter { get; set; }

    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedPaymentId { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Response danh sách giao dịch có phân trang
/// </summary>
public class WalletTransactionListResponse
{
    public List<WalletTransactionResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Response kết quả giao dịch credit/debit
/// </summary>
public class WalletTransactionResult
{
    public bool Success { get; set; }
    public Guid TransactionId { get; set; }
    public long NewBalance { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Response tạo link nạp tiền
/// </summary>
public class WalletTopUpResponse
{
    /// <summary>
    /// ID Payment được tạo
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Order code duy nhất cho giao dịch này
    /// </summary>
    public long OrderCode { get; set; }

    /// <summary>
    /// URL redirect thanh toán (từ Payment Gateway)
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>
    /// QR Code URL (nếu có)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Số tiền nạp (VND)
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Trạng thái: PENDING, PROCESSING, COMPLETED, FAILED
    /// </summary>
    public string Status { get; set; } = "PENDING";

    /// <summary>
    /// Phí giao dịch
    /// </summary>
    public long Fee { get; set; } = 0;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Ghi chú thêm
    /// </summary>
    public string? Message { get; set; }
}

#endregion
