namespace AmarTools.BuildingBlocks.Domain;

/// <summary>
/// Root base class for all domain entities.
/// Provides a strongly-typed primary key and basic timestamp tracking.
/// All entities in every module must inherit from this.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Surrogate primary key. Uses <see cref="Guid"/> to support distributed
    /// generation without a round-trip to the database.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>UTC timestamp set once when the entity is first persisted.</summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp updated on every subsequent save. Null until the first update.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Call inside the infrastructure layer (e.g. <c>SaveChangesAsync</c> interceptor)
    /// to keep <see cref="UpdatedAt"/> accurate without exposing a public setter.
    /// </summary>
    public void SetUpdatedAt(DateTime utcNow) => UpdatedAt = utcNow;
}
