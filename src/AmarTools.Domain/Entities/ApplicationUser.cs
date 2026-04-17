using AmarTools.BuildingBlocks.Domain;

namespace AmarTools.Domain.Entities;

/// <summary>
/// The central user entity for AmarTools.
/// Represents an Event Management Firm (or individual) who owns a subscription.
///
/// Note: This is the *domain* model. The Identity <c>IdentityUser</c> counterpart
/// lives in <c>AmarTools.Infrastructure</c> and is linked by the same <see cref="BaseEntity.Id"/>.
/// </summary>
public sealed class ApplicationUser : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>The user's display name (company or personal name).</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Unique email address used for login and notifications.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Marks this user as a *verified* AmarTools account, making them discoverable
    /// in other users' Contact Books as a platform user rather than a plain contact.
    /// </summary>
    public bool IsVerifiedPlatformUser { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    private readonly List<Event> _ownedEvents = [];
    /// <summary>All events (workspaces) owned by this user.</summary>
    public IReadOnlyCollection<Event> OwnedEvents => _ownedEvents.AsReadOnly();

    private readonly List<EventCoordinator> _coordinatorAssignments = [];
    /// <summary>Events where this user has been assigned as a coordinator.</summary>
    public IReadOnlyCollection<EventCoordinator> CoordinatorAssignments => _coordinatorAssignments.AsReadOnly();

    private readonly List<Subscription> _subscriptions = [];
    /// <summary>Tool subscriptions purchased by this user.</summary>
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    // ── Factory ───────────────────────────────────────────────────────────────

    private ApplicationUser() { } // EF Core

    /// <summary>Creates a new user.</summary>
    public static ApplicationUser Create(string fullName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new ApplicationUser
        {
            FullName = fullName.Trim(),
            Email    = email.Trim().ToLowerInvariant()
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Marks this user as a verified platform user.</summary>
    public void VerifyAsPlatformUser() => IsVerifiedPlatformUser = true;

    /// <summary>Updates the user's display name.</summary>
    public void UpdateFullName(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
    }
}
