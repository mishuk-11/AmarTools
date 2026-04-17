using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Domain;
using AmarTools.Domain.Enums;

namespace AmarTools.Domain.Entities;

/// <summary>
/// RBAC junction entity that grants a <see cref="ApplicationUser"/> (coordinator)
/// access to a specific <see cref="Event"/> with a defined <see cref="CoordinatorRole"/>.
///
/// A coordinator must already exist in the event owner's Contact Book,
/// enforced at the service layer before this entity is created.
/// </summary>
public sealed class EventCoordinator : AuditableEntity
{
    /// <summary>FK to the event this assignment belongs to.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Navigation to the parent event.</summary>
    public Event Event { get; private set; } = null!;

    /// <summary>FK to the user being granted access.</summary>
    public Guid CoordinatorUserId { get; private set; }

    /// <summary>Navigation to the coordinator user.</summary>
    public ApplicationUser CoordinatorUser { get; private set; } = null!;

    /// <summary>The access level granted to this coordinator.</summary>
    public CoordinatorRole Role { get; private set; }

    /// <summary>
    /// Comma-separated fine-grained permission codes from <see cref="AmarTools.BuildingBlocks.Security.Permissions"/>.
    /// Stored as a single string for simplicity; parsed at authorisation time.
    /// Example: <c>"PhotoFrame.Manage,Certificates.Generate"</c>
    /// </summary>
    public string GrantedPermissions { get; private set; } = string.Empty;

    /// <summary>Whether this assignment is currently active.</summary>
    public bool IsActive { get; private set; } = true;

    // ── Factory ───────────────────────────────────────────────────────────────

    private EventCoordinator() { } // EF Core

    /// <summary>Creates a new coordinator assignment.</summary>
    public static EventCoordinator Create(
        Guid eventId, Guid coordinatorUserId, CoordinatorRole role,
        IEnumerable<string>? permissions = null)
    {
        if (eventId == Guid.Empty)           throw new ArgumentException("EventId cannot be empty.");
        if (coordinatorUserId == Guid.Empty)  throw new ArgumentException("CoordinatorUserId cannot be empty.");

        return new EventCoordinator
        {
            EventId              = eventId,
            CoordinatorUserId    = coordinatorUserId,
            Role                 = role,
            GrantedPermissions   = permissions is not null
                                    ? string.Join(',', permissions.Distinct())
                                    : string.Empty,
            IsActive             = true
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Changes the coordinator's role.</summary>
    public void ChangeRole(CoordinatorRole newRole) => Role = newRole;

    /// <summary>
    /// Replaces the full permission set.
    /// Pass an empty collection to revoke all fine-grained permissions.
    /// </summary>
    public void SetPermissions(IEnumerable<string> permissions)
        => GrantedPermissions = string.Join(',', permissions.Distinct());

    /// <summary>Deactivates this coordinator assignment without deleting the record.</summary>
    public Result Revoke()
    {
        if (!IsActive)
            return Error.Failure("Coordinator.AlreadyRevoked", "This coordinator assignment is already revoked.");

        IsActive = false;
        return Result.Ok;
    }

    /// <summary>Reactivates a previously revoked assignment.</summary>
    public void Reinstate() => IsActive = true;

    /// <summary>Returns the parsed list of granted permission codes.</summary>
    public IReadOnlyList<string> GetPermissions()
        => string.IsNullOrWhiteSpace(GrantedPermissions)
            ? []
            : GrantedPermissions.Split(',', StringSplitOptions.RemoveEmptyEntries);
}
