using IdentityService.Application.DTOs;
using IdentityService.Application.Mappers;
using IdentityService.Infrastructure.Repositories.IRepositories;

namespace IdentityService.Application.Services;

/// <summary>
/// Role Business Service
/// Xử lý business logic cho Role
/// </summary>
public class RoleService : Interfaces.IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository) => _roleRepository = roleRepository;

    public async Task<RoleDto?> GetByIdAsync(Guid id)
        => (await _roleRepository.GetByIdAsync(id))?.ToDto();

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
        => (await _roleRepository.GetAllAsync()).Select(r => r.ToDto());

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
        => (await _roleRepository.AddAsync(dto.ToModel())).ToDto();

    public async Task<RoleDto?> UpdateAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null) return null;

        dto.MapToUpdate(role);
        await _roleRepository.UpdateAsync(role);
        
        return (await _roleRepository.GetByIdAsync(id))?.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (!await _roleRepository.ExistsAsync(id)) return false;
        await _roleRepository.DeleteAsync(id);
        return true;
    }
}
