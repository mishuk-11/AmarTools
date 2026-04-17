namespace AmarTools.BuildingBlocks.Domain;

/// <summary>
/// Extends <see cref="BaseEntity"/> with full audit trail columns.
/// Use this for any entity where regulatory or operational accountability
/// requires knowing *who* created or last modified a record.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// The <see cref="Guid"/> of the <c>ApplicationUser</c> who created this record.
    /// Set once on creation; never mutated.
    /// </summary>
    public Guid? CreatedById { get; protected set; }

    /// <summary>
    /// The <see cref="Guid"/> of the <c>ApplicationUser</c> who last modified this record.
    /// Updated alongside <see cref="BaseEntity.UpdatedAt"/>.
    /// </summary>
    public Guid? UpdatedById { get; protected set; }

    /// <summary>
    /// Called by the infrastructure audit interceptor before <c>SaveChangesAsync</c>.
    /// </summary>
    /// <param name="userId">Resolved from the current HTTP context / JWT claim.</param>
    public void SetCreatedBy(Guid userId) => CreatedById = userId;

    /// <summary>
    /// Called by the infrastructure audit interceptor on every subsequent save.
    /// </summary>
    /// <param name="userId">Resolved from the current HTTP context / JWT claim.</param>
    /// <param name="utcNow">Timestamp to stamp alongside the user reference.</param>
    public void SetUpdatedBy(Guid userId, DateTime utcNow)
    {
        UpdatedById = userId;
        SetUpdatedAt(utcNow);
    }
}
