namespace AmarTools.Domain.Enums;

/// <summary>
/// Lifecycle status of a <see cref="AmarTools.Domain.Entities.SubscriptionRequest"/>.
/// </summary>
public enum SubscriptionRequestStatus
{
    /// <summary>Submitted by the user, awaiting admin review.</summary>
    Pending  = 1,

    /// <summary>Approved by an admin. A <see cref="AmarTools.Domain.Entities.Subscription"/> has been created.</summary>
    Approved = 2,

    /// <summary>Rejected by an admin. User must submit a new request.</summary>
    Rejected = 3
}
