using ShipmentService.Application.DTOs;
using ShipmentService.Domain.Entities;

namespace ShipmentService.Application.Mappers;

public static class ProviderServiceMapper
{
    public static ProviderServiceDto ToDto(this ProviderService service)
    {
        if (service == null) throw new ArgumentNullException(nameof(service));

        return new ProviderServiceDto
        {
            ServiceId = service.ServiceId,
            ProviderId = service.ProviderId,
            ProviderName = service.ShippingProvider?.Name,
            Code = service.Code,
            Name = service.Name,
            SpeedLevel = service.SpeedLevel,
            EstimatedDaysMin = service.EstimatedDaysMin,
            EstimatedDaysMax = service.EstimatedDaysMax,
            IsActive = service.IsActive,
            MultiplierFee = service.MultiplierFee,
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    public static ProviderService ToModel(this CreateProviderServiceDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new ProviderService
        {
            ProviderId = dto.ProviderId,
            Code = dto.Code,
            Name = dto.Name,
            SpeedLevel = dto.SpeedLevel,
            EstimatedDaysMin = dto.EstimatedDaysMin,
            EstimatedDaysMax = dto.EstimatedDaysMax,
            IsActive = dto.IsActive,
            MultiplierFee = dto.MultiplierFee
        };
    }

    public static void MapToUpdate(this UpdateProviderServiceDto dto, ProviderService service)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (service == null) throw new ArgumentNullException(nameof(service));

        if (dto.Code != null) service.Code = dto.Code;
        if (dto.Name != null) service.Name = dto.Name;
        if (dto.SpeedLevel != null) service.SpeedLevel = dto.SpeedLevel;
        if (dto.EstimatedDaysMin.HasValue) service.EstimatedDaysMin = dto.EstimatedDaysMin;
        if (dto.EstimatedDaysMax.HasValue) service.EstimatedDaysMax = dto.EstimatedDaysMax;
        if (dto.IsActive.HasValue) service.IsActive = dto.IsActive.Value;
        if (dto.MultiplierFee.HasValue) service.MultiplierFee = dto.MultiplierFee;

        service.UpdatedAt = DateTime.UtcNow;
    }
}
