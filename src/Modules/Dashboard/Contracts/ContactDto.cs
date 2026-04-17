namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>
/// Represents a single entry in the user's Contact Book.
/// The front-end uses <see cref="IsPlatformUser"/> to decide
/// whether to show a "verified" badge and use linked profile data.
/// </summary>
public sealed record ContactDto(
    Guid    Id,
    bool    IsPlatformUser,

    // ── Resolved display fields ───────────────────────────────────────────────
    // For plain contacts: sourced from ContactBookEntry columns.
    // For platform contacts: sourced from the linked ApplicationUser.
    string  DisplayName,
    string  DisplayEmail,
    string? Phone,
    string? Notes,

    // ── Platform-user-only fields ─────────────────────────────────────────────
    Guid?   LinkedUserId,

    DateTime CreatedAt
);
