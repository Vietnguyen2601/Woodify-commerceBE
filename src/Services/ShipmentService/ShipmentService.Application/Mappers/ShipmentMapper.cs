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
            TrackingNumber = shipment.TrackingNumber,
            ProviderServiceCode = shipment.ProviderService?.Code,
            ShippingProviderName = shipment.ProviderService?.ShippingProvider?.Name,
            PickupAddressId = shipment.PickupAddressId,
            DeliveryAddressId = shipment.DeliveryAddressId,
            TotalWeightGrams = shipment.TotalWeightGrams,
            BulkyType = shipment.BulkyType,
            FinalShippingFeeCents = shipment.FinalShippingFeeCents,
            IsFreeShipping = shipment.IsFreeShipping,
            PickupScheduledAt = shipment.PickupScheduledAt,
            PickedUpAt = shipment.PickedUpAt,
            DeliveryEstimatedAt = shipment.DeliveryEstimatedAt,
            Status = shipment.Status,
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
            // TrackingNumber, ProviderServiceId, BulkyType, FinalShippingFeeCents, DeliveryEstimatedAt are set by the service
            PickupAddressId = dto.PickupAddressId,
            DeliveryAddressId = dto.DeliveryAddressId,
            IsFreeShipping = dto.IsFreeShipping
        };
    }

    public static void MapToUpdate(this UpdateShipmentDto dto, Shipment shipment)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (shipment == null) throw new ArgumentNullException(nameof(shipment));

        if (dto.TrackingNumber != null) shipment.TrackingNumber = dto.TrackingNumber;
        if (dto.ProviderServiceId.HasValue) shipment.ProviderServiceId = dto.ProviderServiceId;
        if (dto.PickupAddressId != null) shipment.PickupAddressId = dto.PickupAddressId;
        if (dto.DeliveryAddressId != null) shipment.DeliveryAddressId = dto.DeliveryAddressId;
        if (dto.TotalWeightGrams.HasValue) shipment.TotalWeightGrams = dto.TotalWeightGrams.Value;
        if (dto.BulkyType != null) shipment.BulkyType = dto.BulkyType;
        if (dto.FinalShippingFeeCents.HasValue) shipment.FinalShippingFeeCents = dto.FinalShippingFeeCents.Value;
        if (dto.IsFreeShipping.HasValue) shipment.IsFreeShipping = dto.IsFreeShipping.Value;
        if (dto.PickupScheduledAt.HasValue) shipment.PickupScheduledAt = dto.PickupScheduledAt;
        if (dto.PickedUpAt.HasValue) shipment.PickedUpAt = dto.PickedUpAt;
        if (dto.DeliveryEstimatedAt.HasValue) shipment.DeliveryEstimatedAt = dto.DeliveryEstimatedAt;

        shipment.UpdatedAt = DateTime.UtcNow;
    }
}
