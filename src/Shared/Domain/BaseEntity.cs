namespace Shared.Domain;

/// <summary>
/// Base Entity cho tất cả các entity trong hệ thống
/// Chứa các trường chung: Id, CreatedAt, UpdatedAt
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
