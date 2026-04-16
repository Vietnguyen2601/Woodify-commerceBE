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

#endregion

#region Multi-Order Payment (New)

/// <summary>
/// Request tạo Payment cho multi-order checkout
/// Hỗ trợ 3 phương thức: COD, Wallet, PayOS
/// 
/// Thiết kế đơn giản: Khách hàng chỉ quan tâm thanh toán tổng tiền cho tất cả đơn hàng,
/// không cần chi tiết từng shop. Backend có trách nhiệm verify và phân phối tiền.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    /// Danh sách Order IDs từ CreateOrderFromCart response
    /// </summary>
    public List<Guid> OrderIds { get; set; } = new();

    /// <summary>
    /// Phương thức thanh toán: "COD", "WALLET", "PAYOS"
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// ID tài khoản người mua
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Tổng số tiền cần thanh toán (cents)
    /// Được tính từ sum(Order.TotalAmountVnd) ở CreateOrdersFromCart response
    /// Bao gồm tất cả subtotal + shipping fee cho các đơn hàng
    /// </summary>
    public long TotalAmountVnd { get; set; }

    /// <summary>
    /// Return URL sau khi thanh toán thành công (tùy chọn, dùng cho PayOS)
    /// Mặc định: https://woodify.vn/payment/success
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Cancel URL khi user hủy thanh toán (tùy chọn, dùng cho PayOS)
    /// Mặc định: https://woodify.vn/payment/cancel
    /// </summary>
    public string? CancelUrl { get; set; }
}

/// <summary>
/// Response sau khi tạo Payment
/// Tùy loại payment method mà có thông tin khác nhau
/// </summary>
public class CreatePaymentResponse
{
    /// <summary>
    /// ID payment được tạo
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// List OrderIds liên quan tới payment này
    /// </summary>
    public List<Guid> OrderIds { get; set; } = new();

    /// <summary>
    /// Tổng tiền thanh toán (cents)
    /// </summary>
    public long TotalAmount { get; set; }

    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái thanh toán: "PENDING", "SUCCEEDED", "CREATED", "FAILED"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    // === Dùng cho Wallet ===
    /// <summary>
    /// Số dư còn lại sau khi deduct (chỉ dùng cho WALLET)
    /// </summary>
    public long? RemainingBalance { get; set; }

    // === Dùng cho PayOS ===
    /// <summary>
    /// Mã đơn hàng PayOS (chỉ dùng cho PAYOS)
    /// </summary>
    public long? OrderCode { get; set; }

    /// <summary>
    /// URL thanh toán PayOS (chỉ dùng cho PAYOS)
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// URL QR code PayOS (chỉ dùng cho PAYOS)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Message mô tả
    /// </summary>
    public string? Message { get; set; }
}

#endregion

