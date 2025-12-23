using AccountService.Application.DTOs;
using AccountService.Domain.Entities;

namespace AccountService.Application.Mappers;

public static class RoleMapper
{
    public static RoleDto ToDto(this Role role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role), "Role cannot be null");
        
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            IsActive = role.IsActive
        };
    }

    public static Role ToModel(this CreateRoleDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CreateRoleDto cannot be null.");
        }

        if (string.IsNullOrEmpty(dto.RoleName))
        {
            throw new ArgumentException("RoleName cannot be null or empty.", nameof(dto));
        }

        return new Role
        {
            RoleName = dto.RoleName,
            Description = dto.Description
        };
    }

    public static void MapToUpdate(this UpdateRoleDto dto, Role role)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto), "UpdateRoleDto cannot be null");
        if (role == null) throw new ArgumentNullException(nameof(role), "Role cannot be null");

        if (!string.IsNullOrEmpty(dto.RoleName))
        {
            role.RoleName = dto.RoleName;
        }

        if (!string.IsNullOrEmpty(dto.Description))
        {
            role.Description = dto.Description;
        }

        if (dto.IsActive.HasValue)
        {
            role.IsActive = dto.IsActive.Value;
        }
    }
}
