namespace ShipmentService.Domain.Entities;

/// <summary>
/// Entity Shipment - Bảng Shipments
/// Quản lý thông tin vận chuyển cho từng đơn hàng
/// </summary>
public class Shipment
{
    public Guid ShipmentId { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    public string? TrackingNumber { get; set; }

    public Guid? ProviderServiceId { get; set; }

    // Địa chỉ lưu dưới dạng nvarchar (không dùng uuid)
    public string? PickupAddressId { get; set; }
    public string? DeliveryAddressId { get; set; }

    // Thông tin chi tiết kiện hàng
    public double TotalWeightGrams { get; set; }

    public string? BulkyType { get; set; } // NORMAL, BULKY, SUPER_BULKY

    public long FinalShippingFeeVnd { get; set; }

    public bool IsFreeShipping { get; set; } = false;

    public DateTime? PickupScheduledAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveryEstimatedAt { get; set; }

    public string Status { get; set; } = "DRAFT";
    // DRAFT, PENDING, PICKUP_SCHEDULED, PICKED_UP, IN_TRANSIT,
    // OUT_FOR_DELIVERY, DELIVERED, DELIVERY_FAILED, RETURNING, RETURNED, CANCELLED

    public string? FailureReason { get; set; }
    public string? CancelReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ProviderService? ProviderService { get; set; }
}
