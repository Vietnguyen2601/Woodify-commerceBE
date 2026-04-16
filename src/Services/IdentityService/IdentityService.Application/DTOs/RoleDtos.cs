namespace IdentityService.Application.DTOs;


public class CreateRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}


public class UpdateRoleDto
{
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class RoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
