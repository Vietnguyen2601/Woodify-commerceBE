using IdentityService.Application.DTOs;

namespace IdentityService.Application.Interfaces;

/// <summary>
/// Interface cho Role Business Service
/// </summary>
public interface IRoleService
{
    Task<RoleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleDto dto);
    Task<bool> DeleteAsync(Guid id);
}
