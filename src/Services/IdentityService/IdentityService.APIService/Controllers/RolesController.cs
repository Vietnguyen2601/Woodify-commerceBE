using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;

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
    public async Task<ActionResult<ServiceResult<IEnumerable<RoleDto>>>> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("GetRoleById/{id:guid}")]
    public async Task<ActionResult<ServiceResult<RoleDto>>> GetById(Guid id)
    {
        var result = await _roleService.GetByIdAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpPost("CreateRole")]
    public async Task<ActionResult<ServiceResult<RoleDto>>> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto);
        
        if (result.Status == 201)
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.RoleId }, result);
        
        return BadRequest(result);
    }

    [HttpPut("UpdateRole/{id:guid}")]
    public async Task<ActionResult<ServiceResult<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _roleService.UpdateAsync(id, dto);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }

    [HttpDelete("DeleteRole/{id:guid}")]
    public async Task<ActionResult<ServiceResult>> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        
        if (result.Status == 404)
            return NotFound(result);
        
        if (result.Status != 200)
            return BadRequest(result);
        
        return Ok(result);
    }
}
