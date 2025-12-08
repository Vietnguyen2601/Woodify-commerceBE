namespace AccountService.Common.DTOs;

/// <summary>
/// DTO tạo Role mới
/// </summary>
public class CreateRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// DTO cập nhật Role
/// </summary>
public class UpdateRoleDto
{
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO trả về thông tin Role
/// </summary>
public class RoleDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
