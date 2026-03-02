using ShipmentService.Application.DTOs;
using ShipmentService.Domain.Entities;

namespace ShipmentService.Application.Mappers;

public static class ShippingProviderMapper
{
    public static ShippingProviderDto ToDto(this ShippingProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        return new ShippingProviderDto
        {
            ProviderId = provider.ProviderId,
            Name = provider.Name,
            SupportPhone = provider.SupportPhone,
            SupportEmail = provider.SupportEmail,
            IsActive = provider.IsActive,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt
        };
    }

    public static ShippingProvider ToModel(this CreateShippingProviderDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new ShippingProvider
        {
            Name = dto.Name,
            SupportPhone = dto.SupportPhone,
            SupportEmail = dto.SupportEmail,
            IsActive = dto.IsActive
        };
    }

    public static void MapToUpdate(this UpdateShippingProviderDto dto, ShippingProvider provider)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (provider == null) throw new ArgumentNullException(nameof(provider));

        if (dto.Name != null) provider.Name = dto.Name;
        if (dto.SupportPhone != null) provider.SupportPhone = dto.SupportPhone;
        if (dto.SupportEmail != null) provider.SupportEmail = dto.SupportEmail;
        if (dto.IsActive.HasValue) provider.IsActive = dto.IsActive.Value;

        provider.UpdatedAt = DateTime.UtcNow;
    }
}
