namespace ProductService.Domain.Entities;

/// <summary>
/// Read model synced from OrderService via <c>order.events</c> / <c>order.product.snapshot</c>.
/// </summary>
public class OrderProductMirror
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid AccountId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double SubtotalVnd { get; set; }
    public double TotalAmountVnd { get; set; }
    public long CommissionVnd { get; set; }
    public decimal CommissionRate { get; set; }
    public Guid? VoucherId { get; set; }
    public string? DeliveryAddress { get; set; }
    public string ProviderServiceCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastSnapshotAt { get; set; }
    /// <summary>JSON array of line items (version_id, qty, prices).</summary>
    public string LineItemsJson { get; set; } = "[]";
}
