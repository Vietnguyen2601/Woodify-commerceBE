using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs;

#region Request DTOs

/// <summary>
/// Request tạo link thanh toán PayOS
/// </summary>
public class CreatePaymentLinkRequest
{
    /// <summary>
    /// Mã đơn hàng unique (số nguyên dương)
    /// </summary>
    public long OrderCode { get; set; }

    /// <summary>
    /// Số tiền thanh toán (VND)
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Mô tả đơn hàng (tối đa 25 ký tự)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL redirect khi thanh toán thành công
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL redirect khi hủy thanh toán
    /// </summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// ID đơn hàng trong hệ thống (optional)
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// ID tài khoản người dùng (optional)
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// Tên người mua (optional)
    /// </summary>
    public string? BuyerName { get; set; }

    /// <summary>
    /// Email người mua (optional)
    /// </summary>
    public string? BuyerEmail { get; set; }

    /// <summary>
    /// SĐT người mua (optional)
    /// </summary>
    public string? BuyerPhone { get; set; }
}

#endregion

#region Response DTOs

/// <summary>
/// Response sau khi tạo link thanh toán
/// </summary>
public class CreatePaymentLinkResponse
{
    /// <summary>
    /// ID payment trong DB
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Mã đơn hàng
    /// </summary>
    public long OrderCode { get; set; }

    /// <summary>
    /// URL thanh toán - redirect user đến đây
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL QR code
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Số tiền
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Trạng thái
    /// </summary>
    public string Status { get; set; } = "PENDING";
}

/// <summary>
/// Response khi query thông tin payment
/// </summary>
public class PaymentInfoResponse
{
    public Guid PaymentId { get; set; }
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public int AmountPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Response sau khi xử lý webhook
/// </summary>
public class WebhookProcessResult
{
    public bool Success { get; set; }
    public long OrderCode { get; set; }
    public PaymentStatus NewStatus { get; set; }
    public string Message { get; set; } = string.Empty;
}

#endregion

#region Webhook DTOs

/// <summary>
/// Payload webhook từ API controller
/// </summary>
public class PayOsWebhookRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Desc { get; set; }
    public PayOsWebhookDataDto? Data { get; set; }
    public string? Signature { get; set; }
}

public class PayOsWebhookDataDto
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
    public string? Currency { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public string? CounterAccountBankName { get; set; }
    public string? CounterAccountNumber { get; set; }
    public string? CounterAccountName { get; set; }
    public string? VirtualAccountNumber { get; set; }
    public string? VirtualAccountName { get; set; }
}

#endregion
