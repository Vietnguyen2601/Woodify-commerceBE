using IdentityService.Application.DTOs;
using IdentityService.Application.Mappers;
using IdentityService.Infrastructure.Repositories.IRepositories;
using Shared.Results;

namespace IdentityService.Application.Services;

/// <summary>
/// Role Business Service
/// Xử lý business logic cho Role
/// </summary>
public class RoleService : Interfaces.IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository) => _roleRepository = roleRepository;

    public async Task<ServiceResult<RoleDto>> GetByIdAsync(Guid id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return ServiceResult<RoleDto>.NotFound("Role not found");
        
        return ServiceResult<RoleDto>.Success(role.ToDto());
    }

    public async Task<ServiceResult<IEnumerable<RoleDto>>> GetAllAsync()
    {
        var roles = await _roleRepository.GetAllAsync();
        var roleDtos = roles.Select(r => r.ToDto());
        
        return ServiceResult<IEnumerable<RoleDto>>.Success(roleDtos);
    }

    public async Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto)
    {
        try
        {
            var role = dto.ToModel();
            await _roleRepository.CreateAsync(role);
            
            return ServiceResult<RoleDto>.Created(role.ToDto(), "Role created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<RoleDto>.InternalServerError($"Error creating role: {ex.Message}");
        }
    }

    public async Task<ServiceResult<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return ServiceResult<RoleDto>.NotFound("Role not found");

            dto.MapToUpdate(role);
            await _roleRepository.UpdateAsync(role);
            
            var updatedRole = await _roleRepository.GetByIdAsync(id);
            return ServiceResult<RoleDto>.Success(updatedRole!.ToDto(), "Role updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<RoleDto>.InternalServerError($"Error updating role: {ex.Message}");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return ServiceResult.NotFound("Role not found");
            
            await _roleRepository.RemoveAsync(role);
            return ServiceResult.Success("Role deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult.InternalServerError($"Error deleting role: {ex.Message}");
        }
    }
}
