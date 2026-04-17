using AmarTools.BuildingBlocks.Common;
using AmarTools.BuildingBlocks.Domain;
using AmarTools.Domain.Enums;

namespace AmarTools.Domain.Entities;

/// <summary>
/// A user's request to activate a platform subscription for a chosen package duration.
/// An admin must approve it before any <see cref="Subscription"/> is created.
/// </summary>
public sealed class SubscriptionRequest : AuditableEntity
{
    // ── Valid package durations (days) ────────────────────────────────────────
    public static readonly int[] AllowedPackageDays = [7, 30, 90];

    public Guid                     UserId      { get; private set; }
    public ApplicationUser          User        { get; private set; } = null!;

    /// <summary>7 | 30 | 90 — the package the user chose on registration.</summary>
    public int                      PackageDays { get; private set; }

    public SubscriptionRequestStatus Status     { get; private set; }

    public DateTime                 RequestedAt { get; private set; }
    public DateTime?                ReviewedAt  { get; private set; }

    /// <summary>Admin who approved or rejected this request.</summary>
    public Guid?                    ReviewedById { get; private set; }

    /// <summary>Optional note left by the admin during review.</summary>
    public string?                  AdminNotes  { get; private set; }

    private SubscriptionRequest() { }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<SubscriptionRequest> Create(Guid userId, int packageDays)
    {
        if (userId == Guid.Empty)
            return Error.Validation("SubscriptionRequest.InvalidUser", "User ID cannot be empty.");

        if (!AllowedPackageDays.Contains(packageDays))
            return Error.Validation(
                "SubscriptionRequest.InvalidPackage",
                $"Package must be one of: {string.Join(", ", AllowedPackageDays)} days.");

        return new SubscriptionRequest
        {
            UserId      = userId,
            PackageDays = packageDays,
            Status      = SubscriptionRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result Approve(Guid adminId, string? notes = null)
    {
        if (Status != SubscriptionRequestStatus.Pending)
            return Error.Conflict("SubscriptionRequest.NotPending",
                "Only pending requests can be approved.");

        Status      = SubscriptionRequestStatus.Approved;
        ReviewedAt  = DateTime.UtcNow;
        ReviewedById = adminId;
        AdminNotes  = notes?.Trim();
        return Result.Ok;
    }

    public Result Reject(Guid adminId, string? notes = null)
    {
        if (Status != SubscriptionRequestStatus.Pending)
            return Error.Conflict("SubscriptionRequest.NotPending",
                "Only pending requests can be rejected.");

        Status      = SubscriptionRequestStatus.Rejected;
        ReviewedAt  = DateTime.UtcNow;
        ReviewedById = adminId;
        AdminNotes  = notes?.Trim();
        return Result.Ok;
    }
}
