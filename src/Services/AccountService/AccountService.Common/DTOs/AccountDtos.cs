namespace AccountService.Common.DTOs;

/// <summary>
/// DTO tạo Account mới
/// </summary>
public class CreateAccountDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? RoleId { get; set; }
}

/// <summary>
/// DTO cập nhật Account
/// </summary>
public class UpdateAccountDto
{
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO trả về thông tin Account
/// </summary>
public class AccountDto
{
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
