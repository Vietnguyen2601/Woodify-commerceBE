namespace OrderService.Domain.Entities;

/// <summary>
/// Mirror tối thiểu account info từ IdentityService để OrderService trả thêm accountName mà không gọi HTTP.
/// </summary>
public class AccountDirectoryEntry
{
    public Guid AccountId { get; set; }
    /// <summary>Tên hiển thị (Identity accounts.name, fallback username khi ingest).</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Email đăng nhập từ Identity (mirror).</summary>
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

