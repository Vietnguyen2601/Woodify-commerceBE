namespace AccountService.Domain.Entities;

/// <summary>
/// Entity Role - Bảng Roles
/// Quản lý vai trò người dùng trong hệ thống
/// </summary>
public class Role
{
    public Guid RoleId { get; set; } = Guid.NewGuid();
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
