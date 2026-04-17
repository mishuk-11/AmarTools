namespace AmarTools.BuildingBlocks.Security;

/// <summary>
/// Platform-level role constants shared across all modules and the Web project.
///
/// Roles are coarse-grained (platform-wide). Fine-grained, event-scoped access
/// is handled through <see cref="Permissions"/> stored on <c>EventCoordinator</c>.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Full platform owner. Assigned automatically on registration.
    /// Can create events, manage all tools, and assign coordinators.
    /// </summary>
    public const string Owner = "Owner";

    /// <summary>
    /// Platform super-administrator (LogicThree staff).
    /// Can manage all tenants and platform settings.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Event coordinator. Assigned per-event by an Owner.
    /// Actual capabilities are controlled by the <see cref="Permissions"/> column
    /// on the <c>EventCoordinator</c> entity — not by this role alone.
    /// </summary>
    public const string Coordinator = "Coordinator";

    /// <summary>All defined roles — useful for seeding and policy definitions.</summary>
    public static readonly IReadOnlyList<string> All = [Owner, Admin, Coordinator];
}
