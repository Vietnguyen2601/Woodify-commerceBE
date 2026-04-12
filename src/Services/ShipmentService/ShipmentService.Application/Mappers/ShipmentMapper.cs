using ShipmentService.Application.DTOs;
using ShipmentService.Domain.Entities;

namespace ShipmentService.Application.Mappers;

public static class ShipmentMapper
{
    public static ShipmentDto ToDto(this Shipment shipment)
    {
        if (shipment == null) throw new ArgumentNullException(nameof(shipment));

        return new ShipmentDto
        {
            ShipmentId = shipment.ShipmentId,
            OrderId = shipment.OrderId,
            ShopId = shipment.ShopId,
            TrackingNumber = shipment.TrackingNumber,
            ProviderServiceCode = shipment.ProviderService?.Code,
            ShippingProviderName = shipment.ProviderService?.ShippingProvider?.Name,
            PickupAddress = shipment.PickupAddressId,
            DeliveryAddress = shipment.DeliveryAddressId,
            TotalWeightGrams = shipment.TotalWeightGrams,
            BulkyType = shipment.BulkyType,
            FinalShippingFeeVnd = shipment.FinalShippingFeeVnd,
            IsFreeShipping = shipment.IsFreeShipping,
            PickupScheduledAt = shipment.PickupScheduledAt,
            PickedUpAt = shipment.PickedUpAt,
            DeliveryEstimatedAt = shipment.DeliveryEstimatedAt,
            Status = shipment.Status,
            FailureReason = shipment.FailureReason,
            CancelReason = shipment.CancelReason,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt
        };
    }

    public static Shipment ToModel(this CreateShipmentDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Shipment
        {
            OrderId = dto.OrderId,
            // TrackingNumber, ProviderServiceId, BulkyType, FinalShippingFeeVnd, DeliveryEstimatedAt are set by the service
            PickupAddressId = dto.PickupAddress,
            DeliveryAddressId = dto.DeliveryAddress
        };
    }

    public static void MapToUpdate(this UpdateShipmentDto dto, Shipment shipment)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (shipment == null) throw new ArgumentNullException(nameof(shipment));

        if (dto.TrackingNumber != null) shipment.TrackingNumber = dto.TrackingNumber;
        if (dto.ProviderServiceId.HasValue) shipment.ProviderServiceId = dto.ProviderServiceId;
        if (dto.PickupAddress != null) shipment.PickupAddressId = dto.PickupAddress;
        if (dto.DeliveryAddress != null) shipment.DeliveryAddressId = dto.DeliveryAddress;
        if (dto.TotalWeightGrams.HasValue) shipment.TotalWeightGrams = dto.TotalWeightGrams.Value;
        if (dto.BulkyType != null) shipment.BulkyType = dto.BulkyType;
        if (dto.FinalShippingFeeVnd.HasValue) shipment.FinalShippingFeeVnd = dto.FinalShippingFeeVnd.Value;
        if (dto.IsFreeShipping.HasValue) shipment.IsFreeShipping = dto.IsFreeShipping.Value;
        if (dto.PickupScheduledAt.HasValue) shipment.PickupScheduledAt = dto.PickupScheduledAt;
        if (dto.PickedUpAt.HasValue) shipment.PickedUpAt = dto.PickedUpAt;
        if (dto.DeliveryEstimatedAt.HasValue) shipment.DeliveryEstimatedAt = dto.DeliveryEstimatedAt;
        if (dto.FailureReason != null) shipment.FailureReason = dto.FailureReason;
        if (dto.CancelReason != null) shipment.CancelReason = dto.CancelReason;

        shipment.UpdatedAt = DateTime.UtcNow;
    }
}
