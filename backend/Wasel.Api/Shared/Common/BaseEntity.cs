namespace Wasel.Api.Shared.Common;

/// <summary>
/// Base entity with common audit fields.
/// All domain entities should inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
