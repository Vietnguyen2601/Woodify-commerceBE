namespace PaymentService.Application.DTOs;

/// <summary>
/// PayOS webhook callback data
/// </summary>
public class PayOsWebhookData
{
    public string? Code { get; set; }
    public string? Desc { get; set; }
    public PayOsWebhookPaymentData? Data { get; set; }
    public string? Signature { get; set; }
}

/// <summary>
/// Payment data từ PayOS webhook
/// </summary>
public class PayOsWebhookPaymentData
{
    public long OrderCode { get; set; }
    public int Amount { get; set; }
    public string? Description { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? Reference { get; set; }
    public string? TransactionDateTime { get; set; }
    public string? Status { get; set; }
    public string? Currency { get; set; }
}

/// <summary>
/// Response confirm webhook
/// </summary>
public class PayOsWebhookResponse
{
    public string Code { get; set; } = "00";
    public string Desc { get; set; } = "success";
}
