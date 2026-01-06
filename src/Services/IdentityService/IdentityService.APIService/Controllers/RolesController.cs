using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.APIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("GetAllRoles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("GetRoleById/{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpPost("CreateRole")]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role);
    }

    [HttpPut("UpdateRole/{id:guid}")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateAsync(id, dto);
        if (role == null) return NotFound();
        return Ok(role);
    }

    [HttpDelete("DeleteRole/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
