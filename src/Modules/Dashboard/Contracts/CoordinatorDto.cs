using AmarTools.Domain.Enums;

namespace AmarTools.Modules.Dashboard.Contracts;

/// <summary>
/// Represents a coordinator assigned to a specific event.
/// </summary>
public sealed record CoordinatorDto(
    Guid             Id,
    Guid             EventId,
    Guid             CoordinatorUserId,
    string           FullName,
    string           Email,
    CoordinatorRole  Role,
    IReadOnlyList<string> GrantedPermissions,
    bool             IsActive,
    DateTime         AssignedAt
);
