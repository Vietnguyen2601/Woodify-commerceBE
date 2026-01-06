using IdentityService.Application.DTOs;
using Shared.Results;

namespace IdentityService.Application.Interfaces;

/// <summary>
/// Interface cho Role Business Service
/// </summary>
public interface IRoleService
{
    Task<ServiceResult<RoleDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<IEnumerable<RoleDto>>> GetAllAsync();
    Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto);
    Task<ServiceResult<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto dto);
    Task<ServiceResult> DeleteAsync(Guid id);
}
