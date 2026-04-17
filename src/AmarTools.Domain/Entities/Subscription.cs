using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Domain;
using AmarTools.Domain.Enums;

namespace AmarTools.Domain.Entities;

/// <summary>
/// Records that an <see cref="ApplicationUser"/> has purchased access to a specific <see cref="ToolType"/>.
///
/// A subscription grants the right to activate that tool in any of the user's events.
/// It does NOT enforce per-event usage — <see cref="EventTool"/> handles that.
/// </summary>
public sealed class Subscription : AuditableEntity
{
    /// <summary>FK to the subscribing user.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Navigation to the subscribing user.</summary>
    public ApplicationUser User { get; private set; } = null!;

    /// <summary>The tool this subscription grants access to.</summary>
    public ToolType ToolType { get; private set; }

    /// <summary>UTC timestamp when the subscription was purchased / became active.</summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>
    /// UTC expiry timestamp. <c>null</c> means the subscription is perpetual / lifetime.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>Whether the subscription has been manually revoked by an admin.</summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// <c>true</c> when the subscription is active: not revoked and not expired.
    /// </summary>
    public bool IsActive => !IsRevoked && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);

    // ── Factory ───────────────────────────────────────────────────────────────

    private Subscription() { } // EF Core

    /// <summary>Creates a new subscription record.</summary>
    public static Subscription Create(Guid userId, ToolType toolType, DateTime? expiresAt = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        return new Subscription
        {
            UserId    = userId,
            ToolType  = toolType,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>Extends the subscription expiry.</summary>
    public Result ExtendTo(DateTime newExpiry)
    {
        if (newExpiry <= DateTime.UtcNow)
            return Error.Validation("Subscription.InvalidExpiry", "New expiry must be in the future.");

        ExpiresAt = newExpiry;
        return Result.Ok;
    }

    /// <summary>Administratively revokes an active subscription.</summary>
    public Result Revoke()
    {
        if (IsRevoked)
            return Error.Failure("Subscription.AlreadyRevoked", "This subscription is already revoked.");

        IsRevoked = true;
        return Result.Ok;
    }
}
