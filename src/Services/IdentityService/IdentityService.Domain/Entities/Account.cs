namespace IdentityService.Domain.Entities;

/// <summary>
/// Entity Account - Bảng Accounts
/// Quản lý tài khoản người dùng
/// </summary>
public class Account
{
    public Guid AccountId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? Dob { get; set; }
    public string? Gender { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public virtual Role? Role { get; set; }
}
