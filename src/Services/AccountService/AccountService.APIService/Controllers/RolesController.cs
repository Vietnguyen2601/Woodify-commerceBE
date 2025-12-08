using AccountService.Common.DTOs;
using AccountService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.APIService.Controllers;

/// <summary>
/// Controller quản lý Roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Lấy tất cả roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Lấy role theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDto>> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }

    /// <summary>
    /// Tạo role mới
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = role.RoleId }, role);
    }

    /// <summary>
    /// Cập nhật role
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateAsync(id, dto);
        if (role == null) return NotFound();
        return Ok(role);
    }

    /// <summary>
    /// Xóa role
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
